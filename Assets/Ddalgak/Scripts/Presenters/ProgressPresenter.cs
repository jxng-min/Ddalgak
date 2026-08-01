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
        }

        public override IEnumerator ShowWeekStart(GameRuntimeState runtimeState)
        {
            yield return statView.InitStat();
            yield return calendarView.InitWeek();
        }

        public override IEnumerator ShowWeekSettlement(GameRuntimeState runtimeState)
        {
            yield return calendarView.UpdateWeek(runtimeState);
        }

        public override void Cancel() {}
    }
}