using System;
using System.Collections;
using System.Collections.Generic;
using JxModule;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Ddalgak
{
    public sealed class GameFlowController : MonoBehaviour
    {
        private const int EventsPerWeek = 4;
        private const int FinalWeek = 3;
        private const int RequiredClearEventCount = EventsPerWeek * FinalWeek;
        private const int ConditionalActivationThreshold = 30;
        private const int ConditionalCriticalThreshold = 15;
        private const float ConditionalCriticalWeightMultiplier = 2f;

        [Header("Dependencies")]
        [SerializeField] private EventRepositoryBase eventRepository;
        [SerializeField] private GameFlowPresenterBase presenter;
        [SerializeField] private ActionSequenceRunnerBase actionSequenceRunner;

        private readonly GameRuntimeState _runtimeState = new();
        private Coroutine _gameLoopCoroutine;

        public EGameFlowState CurrentState { get; private set; }
        public GameRuntimeState RuntimeState => _runtimeState;
        public bool IsRunning => _gameLoopCoroutine != null;

        [Button("테스트")]
        public void StartGame()
        {
            if (IsRunning)
            {
                Debug.LogWarning("Game flow is already running.");
                return;
            }

            if (!ValidateDependencies())
            {
                return;
            }

            _gameLoopCoroutine = StartCoroutine(RunGameLoop());
        }

        public void StopGame()
        {
            if (_gameLoopCoroutine != null)
            {
                StopCoroutine(_gameLoopCoroutine);
                _gameLoopCoroutine = null;
            }

            actionSequenceRunner?.Cancel();
            presenter?.Cancel();
            ChangeState(EGameFlowState.None);
        }

        private IEnumerator RunGameLoop()
        {
            ChangeState(EGameFlowState.Initializing);
            _runtimeState.Initialize();
            yield return presenter.ShowGameStart(_runtimeState);

            yield return StartWeek(1);

            while (!_runtimeState.IsGameFinished)
            {
                yield return RunTurn();
            }

            _gameLoopCoroutine = null;
        }

        private IEnumerator RunTurn()
        {
            ChangeState(EGameFlowState.TurnStart);
            TurnContext context = new();

            context.Event = SelectEvent();
            if (context.Event == null)
            {
                Debug.LogError($"No unused { _runtimeState.CurrentSlotType } event is available.");
                StopGame();
                yield break;
            }

            ChangeState(EGameFlowState.EventPresentation);
            yield return presenter.ShowEvent(context.Event);

            if (context.HasChoice)
            {
                yield return WaitForChoice(context);
            }

            if (context.HasAction)
            {
                yield return RunActionSequence(context);
            }

            ChangeState(EGameFlowState.ResultCalculation);
            context.Result = CalculateResult(context);

            ChangeState(EGameFlowState.ResultPresentation);
            yield return presenter.ShowResult(context.Result);

            yield return ApplyStatChanges(context);

            ChangeState(EGameFlowState.GameOverCheck);
            if (context.Result?.IsFatalFailure == true)
            {
                yield return FinishGameOver(EGameOverReason.FatalEventFailure);
                yield break;
            }

            List<EKingdomStatType> collapsedStats = GetCollapsedStats();
            if (collapsedStats.Count > 0)
            {
                bool survived = false;
                yield return ResolveCollapse(collapsedStats, result => survived = result);
                if (!survived)
                {
                    yield break;
                }
            }

            _runtimeState.CompleteEvent(context.Event);

            ChangeState(EGameFlowState.ClearCheck);
            if (CheckClear())
            {
                _runtimeState.SetClear();
                ChangeState(EGameFlowState.Clear);
                yield return presenter.ShowClear();
                ChangeState(EGameFlowState.GovernanceResult);
                yield return presenter.ShowGovernanceResult(_runtimeState.CreateGovernanceResult(true));
                yield break;
            }

            if (_runtimeState.IsWeekCompleted)
            {
                ChangeState(EGameFlowState.WeekSettlement);
                yield return presenter.ShowWeekSettlement(_runtimeState);
                yield return StartWeek(_runtimeState.CurrentWeek + 1);
            }

            ChangeState(EGameFlowState.TurnEnd);
            yield return presenter.WaitForNextTurnInput();
        }

        private IEnumerator StartWeek(int week)
        {
            _runtimeState.StartWeek(week, CreateWeekSlots(week));
            ChangeState(EGameFlowState.WeekStart);
            yield return presenter.ShowWeekStart(_runtimeState);
        }

        private static List<EEventType> CreateWeekSlots(int week)
        {
            switch (week)
            {
                case 1:
                {
                    int actionIndex = Random.Range(1, 3);
                    List<EEventType> slots = new(EventsPerWeek);

                    for (int i = 0; i < EventsPerWeek; i++)
                    {
                        slots.Add(i == actionIndex
                            ? EEventType.ActionChoice
                            : EEventType.NormalChoice);
                    }

                    return slots;
                }

                case 2:
                {
                    List<EEventType> remainingSlots = new()
                    {
                        EEventType.NormalChoice,
                        EEventType.NormalChoice,
                        EEventType.ActionChoice
                    };
                    RandomUtility.Shuffle(remainingSlots);

                    return new List<EEventType>
                    {
                        remainingSlots[0],
                        remainingSlots[1],
                        EEventType.SuddenChoice,
                        remainingSlots[2]
                    };
                }

                case 3:
                {
                    bool normalFirst = Random.value < 0.5f;

                    return new List<EEventType>
                    {
                        normalFirst ? EEventType.NormalChoice : EEventType.ActionChoice,
                        normalFirst ? EEventType.ActionChoice : EEventType.NormalChoice,
                        EEventType.SuddenChoice,
                        EEventType.ActionChoice
                    };
                }

                default:
                    return new List<EEventType>();
            }
        }

        private EventData SelectEvent()
        {
            ChangeState(EGameFlowState.EventSelection);

            IReadOnlyList<EventData> allEvents = eventRepository.GetAllEvents();
            if (allEvents == null)
            {
                return null;
            }

            EEventType slotType = _runtimeState.CurrentSlotType;
            EKingdomStatType? conditionalStat = FindConditionalStat(allEvents, slotType);
            List<EventData> candidates = new();

            foreach (EventData eventData in allEvents)
            {
                if (!CanAppear(eventData, slotType))
                {
                    continue;
                }

                if (eventData.isConditional &&
                    (!conditionalStat.HasValue || eventData.conditionalStat != conditionalStat.Value))
                {
                    continue;
                }

                candidates.Add(eventData);
            }

            return RandomUtility.GetWeightedRandom(candidates, GetEventWeight);
        }

        private bool CanAppear(EventData eventData, EEventType slotType)
        {
            if (eventData == null || eventData.eventType != slotType)
            {
                return false;
            }

            if (_runtimeState.HasCompletedEvent(eventData.eventId))
            {
                return false;
            }

            if (eventData.isConditional && _runtimeState.HasConditionalEventThisWeek)
            {
                return false;
            }

            KingdomStats stats = _runtimeState.Stats;
            if (stats.Treasury < eventData.minTreasury || stats.Treasury > eventData.maxTreasury ||
                stats.PublicSentiment < eventData.minPublicSentiment ||
                stats.PublicSentiment > eventData.maxPublicSentiment ||
                stats.Security < eventData.minSecurity || stats.Security > eventData.maxSecurity)
            {
                return false;
            }

            if (eventData.requiredEventIds == null)
            {
                return true;
            }

            foreach (string requiredEventId in eventData.requiredEventIds)
            {
                if (!_runtimeState.HasCompletedEvent(requiredEventId))
                {
                    return false;
                }
            }

            return true;
        }

        private EKingdomStatType? FindConditionalStat(IReadOnlyList<EventData> allEvents,
                                                       EEventType slotType)
        {
            if (_runtimeState.HasConditionalEventThisWeek)
            {
                return null;
            }

            List<EKingdomStatType> remainingStats = new()
            {
                EKingdomStatType.Treasury,
                EKingdomStatType.PublicSentiment,
                EKingdomStatType.Security
            };

            while (remainingStats.Count > 0)
            {
                int lowestValue = int.MaxValue;
                List<EKingdomStatType> lowestStats = new();

                foreach (EKingdomStatType statType in remainingStats)
                {
                    int value = GetStatValue(statType);
                    if (value < lowestValue)
                    {
                        lowestValue = value;
                        lowestStats.Clear();
                        lowestStats.Add(statType);
                    }
                    else if (value == lowestValue)
                    {
                        lowestStats.Add(statType);
                    }
                }

                if (lowestValue > ConditionalActivationThreshold)
                {
                    return null;
                }

                RandomUtility.Shuffle(lowestStats);
                foreach (EKingdomStatType statType in lowestStats)
                {
                    if (HasConditionalCandidate(allEvents, slotType, statType))
                    {
                        return statType;
                    }

                    remainingStats.Remove(statType);
                }
            }

            return null;
        }

        private bool HasConditionalCandidate(IReadOnlyList<EventData> allEvents,
                                             EEventType slotType,
                                             EKingdomStatType statType)
        {
            foreach (EventData eventData in allEvents)
            {
                if (eventData != null &&
                    eventData.isConditional &&
                    eventData.conditionalStat == statType &&
                    CanAppear(eventData, slotType))
                {
                    return true;
                }
            }

            return false;
        }

        private float GetEventWeight(EventData eventData)
        {
            if (eventData == null || !eventData.isConditional)
            {
                return eventData?.weight ?? 0f;
            }

            return GetStatValue(eventData.conditionalStat) <= ConditionalCriticalThreshold
                ? eventData.weight * ConditionalCriticalWeightMultiplier
                : eventData.weight;
        }

        private int GetStatValue(EKingdomStatType statType)
        {
            return statType switch
            {
                EKingdomStatType.Treasury => _runtimeState.Stats.Treasury,
                EKingdomStatType.PublicSentiment => _runtimeState.Stats.PublicSentiment,
                EKingdomStatType.Security => _runtimeState.Stats.Security,
                _ => int.MaxValue
            };
        }

        private IEnumerator WaitForChoice(TurnContext context)
        {
            ChangeState(EGameFlowState.Choice);

            ChoiceData selectedChoice = null;
            yield return presenter.ShowChoices(context.Event.choices,
                                               choice => selectedChoice = choice);
            yield return new WaitUntil(() => selectedChoice != null);

            context.SelectedChoice = selectedChoice;
            yield return presenter.HideChoices();
        }

        private IEnumerator RunActionSequence(TurnContext context)
        {
            ChangeState(EGameFlowState.Action);

            ActionSequenceResult actionResult = null;
            yield return actionSequenceRunner.Run(context.GetButtonActions(),
                                                  result => actionResult = result);

            context.ActionResult = actionResult ?? ActionSequenceResult.Failure(0, 0);
            _runtimeState.RecordActionResult(context.ActionResult.IsSuccess);
        }

        private static TurnResult CalculateResult(TurnContext context)
        {
            return context.Event.eventType switch
            {
                EEventType.NormalChoice => CalculateNormalChoiceResult(context.SelectedChoice),
                EEventType.ActionChoice => CalculateActionChoiceResult(context),
                EEventType.SuddenChoice => CalculateSuddenChoiceResult(context),
                _ => EmptyResult()
            };
        }

        private static TurnResult CalculateNormalChoiceResult(ChoiceData choice)
        {
            if (choice == null)
            {
                return EmptyResult();
            }

            if (!choice.hasRandomResult)
            {
                return new TurnResult(choice.baseModifier, choice.successResultText, false);
            }

            bool succeeded = Random.value < Mathf.Clamp01(choice.successProbability);
            return new TurnResult(succeeded
                                      ? choice.randomSuccessModifier
                                      : choice.randomFailureModifier,
                                  succeeded
                                      ? choice.successResultText
                                      : choice.failureResultText,
                                  false);
        }

        private static TurnResult CalculateActionChoiceResult(TurnContext context)
        {
            ChoiceData choice = context.SelectedChoice;
            if (choice == null)
            {
                return EmptyResult();
            }

            bool succeeded = context.ActionResult?.IsSuccess == true;
            StatModifier actionModifier = succeeded
                ? choice.actionSuccessModifier
                : choice.actionFailureModifier;

            return new TurnResult(choice.baseModifier + actionModifier,
                                  succeeded ? choice.successResultText : choice.failureResultText,
                                  !succeeded && choice.isFatalOnActionFailure);
        }

        private static TurnResult CalculateSuddenChoiceResult(TurnContext context)
        {
            bool succeeded = context.ActionResult?.IsSuccess == true;
            EventData eventData = context.Event;

            return new TurnResult(succeeded
                                      ? eventData.actionSuccessModifier
                                      : eventData.actionFailureModifier,
                                  succeeded
                                      ? eventData.successResultText
                                      : eventData.failureResultText,
                                  !succeeded && eventData.isFatalOnActionFailure);
        }

        private static TurnResult EmptyResult()
        {
            return new TurnResult(StatModifier.Zero, string.Empty, false);
        }

        private IEnumerator ApplyStatChanges(TurnContext context)
        {
            ChangeState(EGameFlowState.StatUpdate);

            context.StatsBeforeUpdate = _runtimeState.Stats.CreateSnapshot();
            _runtimeState.Stats.Apply(context.Result.FinalModifier);
            context.StatsAfterUpdate = _runtimeState.Stats.CreateSnapshot();

            yield return presenter.AnimateStatChanges(context.StatsBeforeUpdate,
                                                       context.StatsAfterUpdate,
                                                       context.Result.FinalModifier);
        }

        private List<EKingdomStatType> GetCollapsedStats()
        {
            List<EKingdomStatType> collapsedStats = new();

            if (_runtimeState.Stats.Treasury <= 0)
            {
                collapsedStats.Add(EKingdomStatType.Treasury);
            }

            if (_runtimeState.Stats.PublicSentiment <= 0)
            {
                collapsedStats.Add(EKingdomStatType.PublicSentiment);
            }

            if (_runtimeState.Stats.Security <= 0)
            {
                collapsedStats.Add(EKingdomStatType.Security);
            }

            return collapsedStats;
        }

        private IEnumerator ResolveCollapse(IReadOnlyList<EKingdomStatType> collapsedStats,
                                            Action<bool> onCompleted)
        {
            EKingdomStatType collapsedStat = collapsedStats[0];

            if (collapsedStats.Count >= 2 || _runtimeState.HasUsedEmergencyRecovery)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            EKingdomStatType? resourceStat = FindEmergencyResourceStat(collapsedStat);
            if (!resourceStat.HasValue)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            EmergencyRecoveryData recovery =
                GameEndingDataCatalog.GetEmergency(collapsedStat, resourceStat.Value);
            if (recovery == null)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            ChangeState(EGameFlowState.EmergencyRecovery);
            yield return presenter.ShowEmergencyRecovery(recovery);

            bool succeeded = Random.value < Mathf.Clamp01(recovery.successProbability);
            _runtimeState.RecordEmergencyRecovery(succeeded);
            yield return presenter.ShowEmergencyRecoveryResult(recovery, succeeded);

            if (!succeeded)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            KingdomStatsSnapshot before = _runtimeState.Stats.CreateSnapshot();
            _runtimeState.Stats.SetValue(collapsedStat, recovery.recoveryValue);
            _runtimeState.Stats.Apply(recovery.successCost);
            KingdomStatsSnapshot after = _runtimeState.Stats.CreateSnapshot();
            yield return presenter.AnimateStatChanges(before, after, CreateModifier(before, after));

            onCompleted?.Invoke(true);
        }

        private EKingdomStatType? FindEmergencyResourceStat(EKingdomStatType collapsedStat)
        {
            EKingdomStatType[] priority =
            {
                EKingdomStatType.Treasury,
                EKingdomStatType.PublicSentiment,
                EKingdomStatType.Security
            };

            EKingdomStatType? selected = null;
            int selectedValue = 79;

            foreach (EKingdomStatType statType in priority)
            {
                if (statType == collapsedStat)
                {
                    continue;
                }

                int value = GetStatValue(statType);
                if (value > selectedValue)
                {
                    selected = statType;
                    selectedValue = value;
                }
            }

            return selected;
        }

        private IEnumerator FinishGameOver(EGameOverReason reason)
        {
            _runtimeState.SetGameOver(reason);
            ChangeState(EGameFlowState.GameOver);
            yield return presenter.ShowGameOver(GameEndingDataCatalog.GetGameOver(reason));
            ChangeState(EGameFlowState.GovernanceResult);
            yield return presenter.ShowGovernanceResult(_runtimeState.CreateGovernanceResult(false));
        }

        private static EGameOverReason ToGameOverReason(EKingdomStatType statType)
        {
            return statType switch
            {
                EKingdomStatType.Treasury => EGameOverReason.TreasuryDepleted,
                EKingdomStatType.PublicSentiment => EGameOverReason.PublicSentimentCollapsed,
                EKingdomStatType.Security => EGameOverReason.SecurityCollapsed,
                _ => EGameOverReason.FatalEventFailure
            };
        }

        private static StatModifier CreateModifier(KingdomStatsSnapshot before,
                                                   KingdomStatsSnapshot after)
        {
            return new StatModifier(after.Treasury - before.Treasury,
                                    after.PublicSentiment - before.PublicSentiment,
                                    after.Security - before.Security);
        }

        private bool CheckClear()
        {
            return _runtimeState.ProcessedEventCount >= RequiredClearEventCount &&
                   _runtimeState.CurrentWeek == FinalWeek &&
                   _runtimeState.IsWeekCompleted &&
                   _runtimeState.Stats.Treasury > 0 &&
                   _runtimeState.Stats.PublicSentiment > 0 &&
                   _runtimeState.Stats.Security > 0;
        }

        private void ChangeState(EGameFlowState state)
        {
            CurrentState = state;
            Debug.Log($"Game flow state: {CurrentState}");
        }

        private bool ValidateDependencies()
        {
            if (eventRepository == null)
            {
                Debug.LogError("Event repository is missing.");
            }

            if (presenter == null)
            {
                Debug.LogError("Game flow presenter is missing.");
            }

            if (actionSequenceRunner == null)
            {
                Debug.LogError("Action sequence runner is missing.");
            }

            return eventRepository != null && presenter != null && actionSequenceRunner != null;
        }

        private void OnDestroy()
        {
            StopGame();
        }
    }
}
