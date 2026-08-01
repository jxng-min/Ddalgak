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
            yield return ShowPaper(eventData?.title,
                                   eventData?.eventImage,
                                   eventData?.description);
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
            if (result == null || !result.HasActionResult)
            {
                yield break;
            }

            yield return choiceGroupView.ShowResultStamp(result);
        }

        public override IEnumerator ShowEmergencyRecovery(EmergencyRecoveryData data)
        {
            _cancelRequested = false;
            yield return ShowPaper(data?.title,
                                   data?.eventImage,
                                   data?.description);
        }

        public override IEnumerator ShowEmergencyRecoveryResult(EmergencyRecoveryData data,
                                                                  bool succeeded)
        {
            _cancelRequested = false;
            yield return ShowPaper(data?.title,
                                   data?.eventImage,
                                   succeeded ? data?.successText : data?.failureText);
        }

        public override IEnumerator ShowGameOver(GameOverPresentationData data)
        {
            _cancelRequested = false;
            yield return ShowPaper(data?.title,
                                   data?.image,
                                   CombineText(data?.presentation, data?.message));
        }

        public override IEnumerator ShowClear(GameOverPresentationData data)
        {
            _cancelRequested = false;
            yield return ShowPaper(data?.title,
                                   data?.image,
                                   CombineText(data?.presentation, data?.message));
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

        private IEnumerator ShowPaper(string title, Sprite image, string description)
        {
            choiceGroupView.ResetResultStamp();
            eventPaperView.ClearDescription();
            eventPaperView.ShowTitle(title ?? string.Empty);
            yield return eventPaperView.ShowImage(image);
            yield return eventPaperView.ShowDescription(description ?? string.Empty);
        }

        private static string CombineText(string presentation, string message)
        {
            if (string.IsNullOrWhiteSpace(presentation))
            {
                return message ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(message)
                ? presentation
                : $"{presentation}\n\n{message}";
        }
    }
}
