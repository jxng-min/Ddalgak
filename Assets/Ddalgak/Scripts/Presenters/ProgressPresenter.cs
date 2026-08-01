using System.Collections;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public class ProgressPresenter : ProgressPresenterBase
    {
        [BigHeader("UI")]
        [SerializeField] private BackgroundView backgroundView;
        [SerializeField] private CalendarView calendarView;
        [SerializeField] private StatView statView;
        
        public override IEnumerator ShowGameStart(GameRuntimeState runtimeState)
        {
            yield return backgroundView.DrawKingdom();
            yield return statView.InitStat();
        }

        public override IEnumerator ShowWeekStart(GameRuntimeState runtimeState)
        {
            yield return calendarView.UpdateWeek(runtimeState);
        }

        public override IEnumerator ShowWeekSettlement(GameRuntimeState runtimeState)
        {
            yield break;
        }

        public override void Cancel() {}
    }
}