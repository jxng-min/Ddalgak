using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    public sealed class EventPaperView : MonoBehaviour
    {
        [SerializeField] private DropInEffect enterEffect;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private GameObject fadeImage;

        [Header("Fade In")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private Ease fadeInEase = Ease.OutQuad;

        [Header("Typing")]
        [SerializeField] private KoreanTyper koreanTyper;
        [SerializeField, Min(0f)] private float typingInterval = 0.035f;
        [SerializeField, Min(0f)] private float typingStartDelay = 0f;

        private CanvasGroup _fadeCanvasGroup;
        private Coroutine _typingCoroutine;

        public bool IsAnimating { get; private set; }

        private void Awake()
        {
            _fadeCanvasGroup = fadeImage.GetComponent<CanvasGroup>();
        }

        public IEnumerator ShowPaper()
        {
            IsAnimating = true;

            Sequence sequence = DOTween.Sequence();
            sequence.Join(PlayFadeIn());
            sequence.Join(enterEffect.Play());
            yield return sequence.WaitForCompletion();

            IsAnimating = false;
        }

        public void ShowDescriptionText(string description)
        {
            descriptionText.gameObject.SetActive(true);
            StartTyping(description);
        }

        public void HideDescriptionText()
        {
            StopTyping();
            descriptionText.gameObject.SetActive(false);
        }

        public void Cancel()
        {
            IsAnimating = false;
            StopTyping();
            enterEffect.Kill();
            DOTween.Kill(_fadeCanvasGroup);
        }

        private Tween PlayFadeIn()
        {
            DOTween.Kill(_fadeCanvasGroup);
            _fadeCanvasGroup.alpha = 0f;
            return _fadeCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
        }

        private void StartTyping(string text)
        {
            StopTyping();

            text ??= string.Empty;
            if (koreanTyper == null)
            {
                descriptionText.text = text;
                return;
            }

            _typingCoroutine = StartCoroutine(koreanTyper.TypeByInterval(descriptionText, text, typingInterval, typingStartDelay));
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