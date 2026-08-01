using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public abstract class EventPresenterBase : MonoBehaviour
    {
        public abstract IEnumerator ShowEvent(EventData eventData);
        public abstract IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices, Action<ChoiceData> onSelected);
        public abstract IEnumerator HideChoices();
        public abstract IEnumerator ShowResult(TurnResult result);
        public abstract IEnumerator WaitForNextTurnInput();
        public abstract IEnumerator HideEvent();
        public abstract void Cancel();
    }
}