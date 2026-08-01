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
        private Tween _tween;
        private float _restingAnchoredY;
        private bool _hasRestingAnchoredY;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public Tween Play()
        {
            if (!_hasRestingAnchoredY)
            {
                _restingAnchoredY = _rectTransform.anchoredPosition.y;
                _hasRestingAnchoredY = true;
            }

            _tween?.Kill();

            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x,
                                                           _restingAnchoredY - moveDistance);

            _tween = _rectTransform.DOAnchorPosY(_restingAnchoredY, duration).SetEase(ease);
            return _tween;
        }

        public void Kill()
        {
            _tween?.Kill();
            _tween = null;
        }
    }
}
