using DG.Tweening;
using UnityEngine;

namespace Ddalgak
{
    [ManagedEffect("Choice", "ChoiceShowEffect", 0)]
    public sealed class ChoiceShowEffect : MonoBehaviour
    {
        [Header("Show")]
        [SerializeField] private float moveDistance = 12f;
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private float _restingAnchoredY;
        private bool _hasRestingAnchoredY;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public Tween Play()
        {
            if (!_hasRestingAnchoredY)
            {
                _restingAnchoredY = _rectTransform.anchoredPosition.y;
                _hasRestingAnchoredY = true;
            }

            DOTween.Kill(_rectTransform);
            DOTween.Kill(_canvasGroup);

            _canvasGroup.alpha = 0f;
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x,
                                                           _restingAnchoredY - moveDistance);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(_canvasGroup.DOFade(1f, duration).SetEase(ease));
            sequence.Join(_rectTransform.DOAnchorPosY(_restingAnchoredY, duration).SetEase(ease));
            return sequence;
        }

        public Tween PlayHide(float? overrideDuration = null)
        {
            DOTween.Kill(_canvasGroup);
            return _canvasGroup.DOFade(0f, overrideDuration ?? duration).SetEase(ease);
        }

        public void Kill()
        {
            DOTween.Kill(_rectTransform);
            DOTween.Kill(_canvasGroup);
        }
    }
}
