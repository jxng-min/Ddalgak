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
        [SerializeField] private float initDuration;
        [SerializeField] private Ease initEase = Ease.OutBack;
        
        [Header("Update Settings")]
        [SerializeField] private float updateXOffset;
        [SerializeField] private float updateDuration;
        [SerializeField] private float waitDuration;
        [SerializeField] private Ease updateEase = Ease.OutBack;

        public Tween PlayInitCalendarEffect(RectTransform calendarRect, Vector2 originAnchoredPosition)
        {
            return calendarRect.DOAnchorPosX(originAnchoredPosition.x - initXOffset, initDuration)
                               .SetEase(initEase);
        }

        public Tween PlayUpdateCalendarEffect(RectTransform calendarRect, Vector2 originAnchoredPosition)
        {
            
            var sequence = DOTween.Sequence();

            sequence.Join(
                calendarRect.DOAnchorPosX(originAnchoredPosition.x - updateXOffset, updateDuration)
                            .SetEase(updateEase)
            );

            sequence.AppendInterval(waitDuration);

            sequence.Append(
                calendarRect.DOAnchorPosX(originAnchoredPosition.x - initXOffset, updateDuration)
            );

            return sequence;
        }
    }
}