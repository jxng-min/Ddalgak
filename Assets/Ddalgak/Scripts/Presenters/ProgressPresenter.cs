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
        [SerializeField] private EventPaperView eventPaperView;
        [SerializeField] private ChoiceGroupView choiceGroupView;
        
        public override IEnumerator ShowGameStart(GameRuntimeState runtimeState)
        {
            yield return backgroundView.DrawKingdom();
            yield return statView.InitStat();
            yield return eventPaperView.ShowPaper();
            yield return choiceGroupView.ShowInitialButtons();
        }

        public override IEnumerator ShowWeekStart(GameRuntimeState runtimeState)
        {
            StartCoroutine(calendarView.UpdateWeek(runtimeState));
            yield return null;
        }

        public override IEnumerator ShowWeekSettlement(GameRuntimeState runtimeState)
        {
            yield break;
        }

        public override void Cancel()
        {
            eventPaperView.Cancel();
            choiceGroupView.Cancel();
        }
    }
}
