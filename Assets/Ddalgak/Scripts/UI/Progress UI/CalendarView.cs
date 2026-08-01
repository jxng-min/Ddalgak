using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public class CalendarView : LabelBoxView
    {
        [Space(30f)]
        [BigHeader("Effect")]
        [SerializeField] private CalendarEffect calendarEffect;

        private Vector2 _originAnchoredPosition;

        private void Awake()
        {
            _originAnchoredPosition = RectTransform.anchoredPosition;
        }
        
        public IEnumerator UpdateWeek(GameRuntimeState runtimeState)
        {
            Label.text = $"{runtimeState.CurrentWeek}";
            yield return calendarEffect.PlayUpdateCalendarEffect(RectTransform, _originAnchoredPosition).WaitForCompletion();;
        }
    }
}