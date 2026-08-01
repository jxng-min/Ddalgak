using System;
using System.Collections;
using System.Collections.Generic;
using JxModule;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public class EventPresenter : EventPresenterBase
    {
        [BigHeader("UI")]
        [SerializeField] private EventPaperView eventPaperView;
        [SerializeField] private ChoiceGroupView choiceGroupView;

        private bool _cancelRequested;
        
        public override IEnumerator ShowEvent(EventData eventData)
        {
            _cancelRequested = false;
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
            while (!_cancelRequested)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null &&
                    (keyboard.qKey.wasPressedThisFrame ||
                     keyboard.pKey.wasPressedThisFrame ||
                     keyboard.spaceKey.wasPressedThisFrame))
                {
                    choiceGroupView.ResetResultStamp();
                    yield break;
                }

                yield return null;
            }
        }

        public override void Cancel()
        {
            _cancelRequested = true;
        }
    }
}
