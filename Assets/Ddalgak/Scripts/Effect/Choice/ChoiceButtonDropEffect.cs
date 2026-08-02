using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    [ManagedEffect("Choice", "Choice Button Initial Drop", 0)]
    public sealed class ChoiceButtonDropEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Initial Drop")]
        [SerializeField] private float startAnchoredY = 650f;
        [SerializeField] private float targetAnchoredY = -420f;
        [SerializeField] private float duration = 0.45f;
        [SerializeField] private Ease ease = Ease.OutBack;

        private Tween _tween;

        public void Prepare(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            _tween?.Kill();
            _tween = null;
            target.anchoredPosition = new Vector2(target.anchoredPosition.x, startAnchoredY);
        }

        public Tween Play(RectTransform target)
        {
            if (target == null)
            {
                return null;
            }

            Prepare(target);
            SoundManager.Instance.PlaySfx("SFX_ButtonDrop");
            _tween = target.DOAnchorPosY(targetAnchoredY, duration).SetEase(ease);
            return _tween;
        }

        public void Kill()
        {
            _tween?.Kill();
            _tween = null;
        }
    }
}
