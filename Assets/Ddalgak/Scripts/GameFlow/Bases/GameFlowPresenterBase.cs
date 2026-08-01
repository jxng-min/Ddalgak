using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public abstract class GameFlowPresenterBase : MonoBehaviour
    {
        public abstract IEnumerator ShowGameStart(GameRuntimeState runtimeState);
        public abstract IEnumerator ShowWeekStart(GameRuntimeState runtimeState);
        public abstract IEnumerator ShowEvent(EventData eventData);
        public abstract IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices,
                                                Action<ChoiceData> onSelected);
        public abstract IEnumerator HideChoices();
        public abstract IEnumerator ShowResult(TurnResult result);
        public abstract IEnumerator AnimateStatChanges(KingdomStatsSnapshot before,
                                                       KingdomStatsSnapshot after,
                                                       StatModifier modifier);
        public abstract IEnumerator WaitForNextTurnInput();
        public abstract IEnumerator ShowWeekSettlement(GameRuntimeState runtimeState);
        public abstract IEnumerator ShowEmergencyRecovery(EmergencyRecoveryData data);
        public abstract IEnumerator ShowEmergencyRecoveryResult(EmergencyRecoveryData data, bool succeeded);
        public abstract IEnumerator ShowGameOver(GameOverPresentationData data);
        public abstract IEnumerator ShowClear();
        public abstract IEnumerator ShowGovernanceResult(GovernanceResultRecord record);
        public abstract void Cancel();
    }
}
