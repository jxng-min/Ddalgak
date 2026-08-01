using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    [ManagedEffect("Choice", "ChoiceStampEffect", 2)]
    public sealed class ChoiceStampEffect : MonoBehaviour
    {
        [Header("Appear")]
        [SerializeField] private float appearStartScale = 0.6f;
        [SerializeField] private float appearDuration = 0.1f;
        [SerializeField] private Ease appearEase = Ease.OutBack;

        [Header("Fade Out")]
        [SerializeField] private float fadeOutDuration = 0.15f;
        [SerializeField] private Ease fadeOutEase = Ease.InQuad;

        private RectTransform _rectTransform;
        private Graphic _graphic;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _graphic = GetComponent<Graphic>();
            SetAlpha(0f);
        }

        public Sequence Play()
        {
            DOTween.Kill(_rectTransform);
            DOTween.Kill(_graphic);

            _rectTransform.localScale = Vector3.one * appearStartScale;
            SetAlpha(0f);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_rectTransform.DOScale(1f, appearDuration).SetEase(appearEase));
            sequence.Join(_graphic.DOFade(1f, appearDuration));
            sequence.Append(_graphic.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));
            return sequence;
        }

        private void SetAlpha(float alpha)
        {
            Color color = _graphic.color;
            color.a = alpha;
            _graphic.color = color;
        }

        public void Kill()
        {
            DOTween.Kill(_rectTransform);
            DOTween.Kill(_graphic);
        }
    }
}
