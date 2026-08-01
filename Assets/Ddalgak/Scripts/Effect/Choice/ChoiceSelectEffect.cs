using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    [ManagedEffect("Choice", "선택 확정 효과", 1)]
    public sealed class ChoiceSelectEffect : MonoBehaviour
    {
        [Header("Scale Punch")]
        [SerializeField] private float shrinkScale = 0.92f;
        [SerializeField] private float shrinkDuration = 0.06f;
        [SerializeField] private float holdDuration = 0.08f;
        [SerializeField] private float overshootScale = 1.05f;
        [SerializeField] private float overshootDuration = 0.06f;
        [SerializeField] private float settleDuration = 0.05f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        [Header("Highlight")]
        [SerializeField] private Graphic borderHighlight;
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private Color highlightTextColor = Color.white;

        [Header("Dim")]
        [SerializeField] private float dimAlpha = 0.45f;
        [SerializeField] private float dimDuration = 0.2f;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Color _originalTextColor;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (targetText != null)
            {
                _originalTextColor = targetText.color;
            }

            if (borderHighlight != null)
            {
                borderHighlight.enabled = false;
            }
        }

        public Sequence PlaySelect()
        {
            DOTween.Kill(_rectTransform);

            if (borderHighlight != null)
            {
                borderHighlight.enabled = true;
            }

            if (targetText != null)
            {
                targetText.color = highlightTextColor;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_rectTransform.DOScale(shrinkScale, shrinkDuration).SetEase(ease));
            sequence.AppendInterval(holdDuration);
            sequence.Append(_rectTransform.DOScale(overshootScale, overshootDuration).SetEase(ease));
            sequence.Append(_rectTransform.DOScale(1f, settleDuration).SetEase(ease));
            return sequence;
        }

        public Tween PlayDim()
        {
            DOTween.Kill(_canvasGroup);
            return _canvasGroup.DOFade(dimAlpha, dimDuration);
        }

        public void ResetVisual()
        {
            DOTween.Kill(_rectTransform);
            DOTween.Kill(_canvasGroup);

            if (borderHighlight != null)
            {
                borderHighlight.enabled = false;
            }

            if (targetText != null)
            {
                targetText.color = _originalTextColor;
            }

            if (_rectTransform != null)
            {
                _rectTransform.localScale = Vector3.one;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        public void Kill()
        {
            DOTween.Kill(_rectTransform);
            DOTween.Kill(_canvasGroup);
        }
    }
}