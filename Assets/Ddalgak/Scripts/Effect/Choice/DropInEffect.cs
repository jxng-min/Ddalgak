using DG.Tweening;
using UnityEngine;

namespace Ddalgak
{
    [ManagedEffect("Common", "낙하 등장 효과", 0)]
    public sealed class DropInEffect : MonoBehaviour
    {
        [Header("Enter")]
        [SerializeField] private float startOffsetY = 1000f;
        [SerializeField] private float duration = 0.45f;
        [SerializeField] private Ease ease = Ease.OutBack;

        private RectTransform _rectTransform;
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

            DOTween.Kill(_rectTransform);

            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x,
                                                           _restingAnchoredY + startOffsetY);

            return _rectTransform.DOAnchorPosY(_restingAnchoredY, duration).SetEase(ease);
        }

        public void Kill()
        {
            DOTween.Kill(_rectTransform);
        }
    }
}