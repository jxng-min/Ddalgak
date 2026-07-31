using System.Collections;
using System.Collections.Generic;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public sealed class GameFlowController : MonoBehaviour
    {
        private const int RequiredClearEventCount = 20;

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
                Debug.LogError("No event satisfying the current conditions.");
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
            context.GameOverReason = CheckGameOver(context.Result);
            if (context.GameOverReason != EGameOverReason.None)
            {
                _runtimeState.SetGameOver(context.GameOverReason);
                ChangeState(EGameFlowState.GameOver);
                yield return presenter.ShowGameOver(context.GameOverReason);
                yield break;
            }

            _runtimeState.CompleteEvent(context.Event.eventId);
            yield return UpdateProcedureLevel();

            ChangeState(EGameFlowState.ClearCheck);
            if (CheckClear())
            {
                _runtimeState.SetClear();
                ChangeState(EGameFlowState.Clear);
                yield return presenter.ShowClear();
                yield break;
            }

            ChangeState(EGameFlowState.TurnEnd);
            yield return presenter.WaitForNextTurnInput();
        }

        private EventData SelectEvent()
        {
            ChangeState(EGameFlowState.EventSelection);

            IReadOnlyList<EventData> allEvents = eventRepository.GetAllEvents();
            if (allEvents == null)
            {
                return null;
            }

            List<EventData> candidates = new();
            foreach (EventData eventData in allEvents)
            {
                if (CanAppear(eventData))
                {
                    candidates.Add(eventData);
                }
            }

            return RandomUtility.GetWeightedRandom(candidates, eventData => eventData.weight);
        }

        private bool CanAppear(EventData eventData)
        {
            if (eventData == null || _runtimeState.IsRecentEvent(eventData.eventId))
            {
                return false;
            }

            if (_runtimeState.ProcedureLevel < eventData.minProcedureLevel ||
                _runtimeState.ProcedureLevel > eventData.maxProcedureLevel)
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
            return choice == null
                ? EmptyResult()
                : new TurnResult(choice.baseModifier, choice.successResultText, false);
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

        private EGameOverReason CheckGameOver(TurnResult result)
        {
            if (result?.IsFatalFailure == true)
            {
                return EGameOverReason.FatalEventFailure;
            }

            if (_runtimeState.Stats.Treasury <= 0)
            {
                return EGameOverReason.TreasuryDepleted;
            }

            if (_runtimeState.Stats.PublicSentiment <= 0)
            {
                return EGameOverReason.PublicSentimentCollapsed;
            }

            if (_runtimeState.Stats.Security <= 0)
            {
                return EGameOverReason.SecurityCollapsed;
            }
            
            return EGameOverReason.None;
        }

        private IEnumerator UpdateProcedureLevel()
        {
            ChangeState(EGameFlowState.ProcedureCheck);

            var previousLevel = _runtimeState.ProcedureLevel;
            var currentLevel = CalculateProcedureLevel(_runtimeState.ProcessedEventCount);
            _runtimeState.SetProcedureLevel(currentLevel);

            if (currentLevel > previousLevel)
            {
                yield return presenter.ShowProcedureLevelUp(previousLevel, currentLevel);
            }
        }

        private static int CalculateProcedureLevel(int processedEventCount)
        {
            if (processedEventCount >= 18)
            {
                return 4;
            }

            if (processedEventCount >= 13)
            {
                return 3;
            }

            if (processedEventCount >= 8)
            {
                return 2;
            }

            if (processedEventCount >= 4)
            {
                return 1;
            }
            
            return 0;
        }

        private bool CheckClear()
        {
            return _runtimeState.ProcessedEventCount >= RequiredClearEventCount &&
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
