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

        private CanvasGroup _fadeCanvasGroup;

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
            descriptionText.text = description ?? string.Empty;
            descriptionText.gameObject.SetActive(true);
        }

        public void HideDescriptionText()
        {
            descriptionText.gameObject.SetActive(false);
        }

        public void Cancel()
        {
            IsAnimating = false;
            enterEffect.Kill();
            DOTween.Kill(_fadeCanvasGroup);
        }

        private Tween PlayFadeIn()
        {
            DOTween.Kill(_fadeCanvasGroup);
            _fadeCanvasGroup.alpha = 0f;
            return _fadeCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
        }
    }
}