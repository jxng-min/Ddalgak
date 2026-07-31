using System;
using System.Collections;
using System.Collections.Generic;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public sealed class GameFlowController : MonoBehaviour
    {
        private const int EventsPerWeek = 4;
        private const int FinalWeek = 3;
        private const int RequiredClearEventCount = EventsPerWeek * FinalWeek;

        [Header("Dependencies")]
        [SerializeField] private EventRepositoryBase eventRepository;
        [SerializeField] private GameFlowPresenterBase presenter;
        [SerializeField] private ActionSequenceRunnerBase actionSequenceRunner;

        private readonly GameRuntimeState _runtimeState = new();
        private readonly WeekSlotPlanner _weekSlotPlanner = new();
        private readonly EventSelector _eventSelector = new();
        private readonly TurnResultCalculator _turnResultCalculator = new();
        private Coroutine _gameLoopCoroutine;

        public EGameFlowState CurrentState { get; private set; }
        public GameRuntimeState RuntimeState => _runtimeState;
        public bool IsRunning => _gameLoopCoroutine != null;
        public EDebugOutcomeMode DebugProbabilityMode { get; set; }
        public bool DebugForceConditionalEvent { get; set; }

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

        public void RestartGame()
        {
            StopGame();
            StartGame();
        }

        public void DebugSetStats(int treasury, int publicSentiment, int security)
        {
            _runtimeState.Stats.SetValue(EKingdomStatType.Treasury, treasury);
            _runtimeState.Stats.SetValue(EKingdomStatType.PublicSentiment, publicSentiment);
            _runtimeState.Stats.SetValue(EKingdomStatType.Security, security);
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

            ChangeState(EGameFlowState.EventSelection);
            context.Event = _eventSelector.Select(eventRepository.GetAllEvents(),
                                                  _runtimeState,
                                                  DebugForceConditionalEvent);
            if (context.Event == null)
            {
                Debug.LogError($"No unused {_runtimeState.CurrentSlotType} event is available.");
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
            context.Result = _turnResultCalculator.Calculate(context, DebugProbabilityMode);

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
            _runtimeState.StartWeek(week, _weekSlotPlanner.Create(week, _runtimeState));
            ChangeState(EGameFlowState.WeekStart);
            yield return presenter.ShowWeekStart(_runtimeState);
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
            var collapsedStat = collapsedStats[0];
            if (collapsedStats.Count >= 2 || _runtimeState.HasUsedEmergencyRecovery)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            var resourceStat = FindEmergencyResourceStat(collapsedStat);
            if (!resourceStat.HasValue)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            var recovery = GameEndingDataCatalog.GetEmergency(collapsedStat, resourceStat.Value);
            if (recovery == null)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            ChangeState(EGameFlowState.EmergencyRecovery);
            yield return presenter.ShowEmergencyRecovery(recovery);

            var succeeded = _turnResultCalculator.RollProbability(recovery.successProbability,
                                                                       DebugProbabilityMode);
            _runtimeState.RecordEmergencyRecovery(succeeded);
            yield return presenter.ShowEmergencyRecoveryResult(recovery, succeeded);

            if (!succeeded)
            {
                yield return FinishGameOver(ToGameOverReason(collapsedStat));
                onCompleted?.Invoke(false);
                yield break;
            }

            var before = _runtimeState.Stats.CreateSnapshot();
            _runtimeState.Stats.SetValue(collapsedStat, recovery.recoveryValue);
            _runtimeState.Stats.Apply(recovery.successCost);
            
            var after = _runtimeState.Stats.CreateSnapshot();
            
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
            var selectedValue = 79;

            foreach (EKingdomStatType statType in priority)
            {
                if (statType == collapsedStat)
                {
                    continue;
                }

                var value = GetStatValue(statType);
                if (value > selectedValue)
                {
                    selected = statType;
                    selectedValue = value;
                }
            }

            return selected;
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
