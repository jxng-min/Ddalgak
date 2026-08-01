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
            yield return eventPaperEffect.PlayShowPaperEffect(
                RectTransform, 
                _originAnchoredPosition, 
                frameImage).WaitForCompletion();
        }

        public void ShowTitle(string eventTitle)
        {
            titleLabel.text = eventTitle;
        }

        public IEnumerator ShowImage(Sprite eventSprite)
        {
            eventImage.Image.sprite = eventSprite;
            yield return eventPaperEffect.PlayShowImageEffect(eventImage.CanvasGroup);
        }

        public IEnumerator ShowDescription(string description)
        {
            StopTyping();

            description ??= string.Empty;
            if (koreanTyper == null)
            {
                descriptionLabel.text = description;
                yield break;
            }

            _typingCoroutine = StartCoroutine(koreanTyper.TypeByInterval(descriptionLabel, description, typingInterval, typingStartDelay));
            yield return _typingCoroutine;
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
