using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    [ManagedEffect("Progress", "Calendar", 1)]
    public class CalendarEffect : MonoBehaviour
    {
        [BigHeader("settings")]
        [Header("Initialize Settings")]
        [SerializeField] private float initXOffset;
        
        [Header("Update Settings")]
        [SerializeField] private float updateXOffset;
        [SerializeField] private float updateDuration;
        [SerializeField] private float waitDuration;
        [SerializeField] private Ease updateEase = Ease.OutBack;

        public Tween PlayUpdateCalendarEffect(RectTransform calendarRect, Vector2 originAnchoredPosition)
        {
            
            var sequence = DOTween.Sequence();

            sequence.Join(
                calendarRect.DOAnchorPosX(originAnchoredPosition.x - updateXOffset - initXOffset, updateDuration)
                            .SetEase(updateEase)
            );

            sequence.AppendInterval(waitDuration);

            sequence.Append(
                calendarRect.DOAnchorPosX(originAnchoredPosition.x - initXOffset, updateDuration)
                            .SetEase(updateEase)
            );

            return sequence;
        }
    }
}