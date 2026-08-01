using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    public sealed class ButtonActionView : MonoBehaviour
    {
        [SerializeField] private Image actionImage;

        [SerializeField] private Sprite[] singlePressFrames;
        [SerializeField] private Sprite[] pressFrames;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float frameInterval = 0.05f;

        private Coroutine _animationCoroutine;

        public void Show(EButtonActionType actionType, float duration = 0f)
        {
            actionImage.gameObject.SetActive(true);
            StopAnimation();

            switch (actionType)
            {
                case EButtonActionType.SinglePress:
                    _animationCoroutine = StartCoroutine(RunLoopingFrameAnimation(singlePressFrames));
                    break;
                case EButtonActionType.RapidPress:
                    _animationCoroutine = StartCoroutine(RunLoopingFrameAnimation(pressFrames));
                    break;
                case EButtonActionType.Timing:
                    _animationCoroutine = StartCoroutine(RunOneShotFrameAnimation(pressFrames, duration));
                    break;
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

        private IEnumerator RunLoopingFrameAnimation(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0)
            {
                yield break;
            }

            int frameIndex = 0;
            while (true)
            {
                actionImage.sprite = frames[frameIndex];
                frameIndex = (frameIndex + 1) % frames.Length;
                yield return new WaitForSeconds(frameInterval);
            }
        }

        private IEnumerator RunOneShotFrameAnimation(Sprite[] frames, float duration)
        {
            if (frames == null || frames.Length == 0 || duration <= 0f)
            {
                yield break;
            }

            float perFrameDuration = duration / frames.Length;
            for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
            {
                actionImage.sprite = frames[frameIndex];
                yield return new WaitForSeconds(perFrameDuration);
            }
        }
    }
}
