using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    public sealed class ButtonActionView : MonoBehaviour
    {
        [SerializeField] private Image actionImage;

        [SerializeField] private Sprite[] singlePressFrames;
        [SerializeField] private Sprite[] rapidPressFrames;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float frameInterval = 0.05f;

        private Coroutine _animationCoroutine;

        public void Show(EButtonActionType actionType)
        {
            actionImage.gameObject.SetActive(true);
            StopAnimation();

            Sprite[] frames = actionType switch
            {
                EButtonActionType.SinglePress => singlePressFrames,
                EButtonActionType.RapidPress => rapidPressFrames,
                _ => null
            };

            if (frames != null && frames.Length > 0)
            {
                _animationCoroutine = StartCoroutine(RunFrameAnimation(frames));
            }
        }

        public void Hide()
        {
            StopAnimation();
            actionImage.gameObject.SetActive(false);
        }

        private void StopAnimation()
        {
            if (_animationCoroutine == null)
            {
                return;
            }

            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }

        private IEnumerator RunFrameAnimation(Sprite[] frames)
        {
            int frameIndex = 0;
            while (true)
            {
                actionImage.sprite = frames[frameIndex];
                frameIndex = (frameIndex + 1) % frames.Length;
                yield return new WaitForSeconds(frameInterval);
            }
        }
    }
}