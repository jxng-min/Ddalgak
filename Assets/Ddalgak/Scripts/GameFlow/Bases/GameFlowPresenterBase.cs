using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public abstract class GameFlowPresenterBase : MonoBehaviour
    {
        public abstract IEnumerator ShowGameStart(GameRuntimeState runtimeState);
        public abstract IEnumerator ShowEvent(EventData eventData);
        public abstract IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices,
                                                Action<ChoiceData> onSelected);
        public abstract IEnumerator HideChoices();
        public abstract IEnumerator ShowResult(TurnResult result);
        public abstract IEnumerator AnimateStatChanges(KingdomStatsSnapshot before,
                                                        KingdomStatsSnapshot after,
                                                        StatModifier modifier);
        public abstract IEnumerator WaitForNextTurnInput();
        public abstract IEnumerator ShowProcedureLevelUp(int previousLevel, int currentLevel);
        public abstract IEnumerator ShowGameOver(EGameOverReason reason);
        public abstract IEnumerator ShowClear();
        public abstract void Cancel();
    }
}
