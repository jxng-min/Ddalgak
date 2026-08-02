using System.Collections;
using DG.Tweening;
using JxModule;
using TMPro;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EventPaperView : ViewBase
    {
        [BigHeader("UI")]
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private ImageView eventImage;
        [SerializeField] private ImageView frameImage;
        [SerializeField] private TMP_Text descriptionLabel;

        [Space(30f)]
        [BigHeader("Effect")]
        [SerializeField] private EventPaperEffect eventPaperEffect;

        [Header("Typing")]
        [SerializeField] private KoreanTyper koreanTyper;
        [SerializeField, Min(0f)] private float typingInterval = 0.035f;
        [SerializeField, Min(0f)] private float typingStartDelay = 0f;

        private Vector2 _originAnchoredPosition;
        private Coroutine _typingCoroutine;
        private Tween _showTween;

        private void Awake()
        {
            _originAnchoredPosition = RectTransform.anchoredPosition;
        }

        public IEnumerator ShowPaper()
        {
            SoundManager.Instance.PlaySfx("SFX_EventOpen");

            _showTween?.Kill();
            _showTween = eventPaperEffect.PlayShowPaperEffect(
                RectTransform,
                _originAnchoredPosition,
                frameImage);

            yield return _showTween.WaitForCompletion();
            _showTween = null;
        }

        public void ShowTitle(string eventTitle)
        {
            titleLabel.text = eventTitle;
        }

        public void ClearDescription()
        {
            StopTyping();
            descriptionLabel.text = string.Empty;
            descriptionLabel.gameObject.SetActive(true);
        }

        public IEnumerator ShowImage(Sprite eventSprite)
        {
            _showTween?.Kill();

            CanvasGroup imageGroup = eventImage.CanvasGroup;
            imageGroup.DOKill();
            imageGroup.alpha = 0f;
            eventImage.Image.sprite = eventSprite;

            yield return null;

            _showTween = eventPaperEffect.PlayShowImageEffect(imageGroup);

            yield return _showTween.WaitForCompletion();
            _showTween = null;
        }

        public IEnumerator ShowDescription(string description)
        {
            StopTyping();
            descriptionLabel.gameObject.SetActive(true);
            description ??= string.Empty;
            if (koreanTyper == null)
            {
                descriptionLabel.text = description;
                yield break;
            }

            _typingCoroutine = StartCoroutine(koreanTyper.TypeByInterval(descriptionLabel, description, typingInterval, typingStartDelay));
            yield return _typingCoroutine;
            _typingCoroutine = null;
        }

        public void HideDescriptionText()
        {
            StopTyping();
            descriptionLabel.gameObject.SetActive(false);
        }

        public void Cancel()
        {
            StopTyping();
            _showTween?.Kill();
            _showTween = null;
        }

        private void StopTyping()
        {
            if (_typingCoroutine == null)
            {
                return;
            }

            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
    }
}
