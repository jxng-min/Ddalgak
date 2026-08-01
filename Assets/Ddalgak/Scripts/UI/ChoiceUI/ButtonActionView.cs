using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ddalgak
{
    public sealed class ButtonActionView : MonoBehaviour
    {
        [SerializeField] private Image pActionImage;
        [SerializeField] private Image qActionImage;
        [SerializeField] private Image spaceActionImage;

        [SerializeField] private Sprite[] singlePressFrames;
        [SerializeField] private Sprite[] pressFrames;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float frameInterval = 0.05f;

        private Image _activeImage;
        private Coroutine _animationCoroutine;

        public void Show(EButtonActionType actionType, Key key, float duration = 0f)
        {
            StopAnimation();
            HideAllImages();

            _activeImage = GetImageForKey(key);
            if (_activeImage == null)
            {
                return;
            }

            _activeImage.gameObject.SetActive(true);

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
            HideAllImages();
            _activeImage = null;
        }

        private Image GetImageForKey(Key key)
        {
            return key switch
            {
                Key.P => pActionImage,
                Key.Q => qActionImage,
                Key.Space => spaceActionImage,
                _ => null
            };
        }

        private void HideAllImages()
        {
            pActionImage.gameObject.SetActive(false);
            qActionImage.gameObject.SetActive(false);
            spaceActionImage.gameObject.SetActive(false);
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
                _activeImage.sprite = frames[frameIndex];
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
                _activeImage.sprite = frames[frameIndex];
                yield return new WaitForSeconds(perFrameDuration);
            }
        }
    }
}
