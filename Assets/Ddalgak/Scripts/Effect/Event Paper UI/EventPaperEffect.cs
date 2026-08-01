using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    [ManagedEffect("Event", "Event Paper", 0)]
    public class EventPaperEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Show Paper")]
        [SerializeField] private float showYOffset = -1100f;
        [SerializeField] private float showDuration = 0.5f;
        [SerializeField] private Ease showEase = Ease.Unset;
        
        
        public Tween PlayShowPaperEffect(RectTransform paperRect,
                                         Vector2 originAnchoredPosition,
                                         ImageView frameImageView)
        {
            var sequence = DOTween.Sequence();

            sequence.Join(
                paperRect.DOAnchorPosY(originAnchoredPosition.y + showYOffset, showDuration)
            );

            sequence.Join(
                frameImageView.CanvasGroup.DOFade(1f, showDuration)
            );

            return sequence;
        }

        public Tween PlayShowImageEffect(CanvasGroup imageGroup)
        {
            return imageGroup.DOFade(1f, showDuration);
        }
    }
}
