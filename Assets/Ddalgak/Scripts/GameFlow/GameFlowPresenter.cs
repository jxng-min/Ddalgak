using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public class GameFlowPresenter : GameFlowPresenterBase
    {
        [SerializeField] private EventPresenterBase eventPresenter;
        [SerializeField] private StatPresenterBase statPresenter;
        [SerializeField] private ProgressPresenterBase progressPresenter;
        [SerializeField] private EndingPresenterBase endingPresenter;

        public override IEnumerator ShowGameStart(GameRuntimeState runtimeState)
        {
            statPresenter.SetStats(runtimeState.Stats.CreateSnapshot());
            return progressPresenter.ShowGameStart(runtimeState);
        }

        public override IEnumerator ShowWeekStart(GameRuntimeState runtimeState)
        {
            return progressPresenter.ShowWeekStart(runtimeState);
        }

        public override IEnumerator ShowEvent(EventData eventData)
        {
            return eventPresenter.ShowEvent(eventData);
        }

        public override IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices, Action<ChoiceData> onSelected)
        {
            return eventPresenter.ShowChoices(choices, onSelected);
        }

        public override IEnumerator HideChoices()
        {
            return eventPresenter.HideChoices();
        }

        public override IEnumerator ShowResult(TurnResult result)
        {
            return eventPresenter.ShowResult(result);
        }

        public override IEnumerator AnimateStatChanges(KingdomStatsSnapshot before,
                                                       KingdomStatsSnapshot after,
                                                       StatModifier modifier)
        {
            return statPresenter.AnimateStatChanges(before, after, modifier);
        }

        public override IEnumerator WaitForNextTurnInput()
        {
            return eventPresenter.WaitForNextTurnInput();
        }

        public override IEnumerator HideEvent()
        {
            return eventPresenter.HideEvent();
        }

        public override IEnumerator ShowWeekSettlement(GameRuntimeState runtimeState)
        {
            return progressPresenter.ShowWeekSettlement(runtimeState);
        }

        public override IEnumerator ShowEmergencyRecovery(EmergencyRecoveryData data)
        {
            return endingPresenter.ShowEmergencyRecovery(data);
        }

        public override IEnumerator ShowEmergencyRecoveryResult(EmergencyRecoveryData data, bool succeeded)
        {
            return endingPresenter.ShowEmergencyRecoveryResult(data, succeeded);
        }

        public override IEnumerator ShowGameOver(GameOverPresentationData data)
        {
            return endingPresenter.ShowGameOver(data);
        }

        public override IEnumerator ShowClear()
        {
            return endingPresenter.ShowClear();
        }

        public override IEnumerator ShowGovernanceResult(GovernanceResultRecord record)
        {
            return endingPresenter.ShowGovernanceResult(record);
        }

        public override void Cancel()
        {
            eventPresenter.Cancel();
            statPresenter.Cancel();
            progressPresenter.Cancel();
            endingPresenter.Cancel();
        }
    }
}