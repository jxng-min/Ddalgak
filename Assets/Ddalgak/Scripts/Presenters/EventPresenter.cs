using System;
using System.Collections;
using System.Collections.Generic;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public class EventPresenter : EventPresenterBase
    {
        [BigHeader("UI")]
        [SerializeField] private EventPaperView eventPaperView;
        [SerializeField] private ChoiceGroupView choiceGroupView;
        
        public override IEnumerator ShowEvent(EventData eventData)
        {
            eventPaperView.ShowTitle(eventData.title);
            yield return eventPaperView.ShowImage(eventData.eventImage);
            yield return eventPaperView.ShowDescription(eventData.description);
        }

        public override IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices, Action<ChoiceData> onSelected)
        {
            yield return choiceGroupView.ShowChoices(choices, onSelected);
        }

        public override IEnumerator HideChoices()
        {
            yield return choiceGroupView.HideChoices();
        }

        public override IEnumerator ShowResult(TurnResult result)
        {
            yield return choiceGroupView.ShowResultStamp(result);
        }

        public override IEnumerator WaitForNextTurnInput()
        {
            yield break;
        }

        public override void Cancel() {}
    }
}