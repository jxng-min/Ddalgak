using System.Collections;
using UnityEngine;

namespace Ddalgak
{
    public class StatPresenter : StatPresenterBase
    {
        [SerializeField] private StatSlotView[] statSlotViews;

        private StatSlotView TreasurySlotView => statSlotViews[0];
        private StatSlotView SentimentSlotView => statSlotViews[1];
        private StatSlotView SecuritySlotView => statSlotViews[2];


        public override void SetStats(KingdomStatsSnapshot stats)
        {
            TreasurySlotView.SetRate(ToRate(stats.Treasury));
            SentimentSlotView.SetRate(ToRate(stats.PublicSentiment));
            SecuritySlotView.SetRate(ToRate(stats.Security));
        }
        
        public override IEnumerator AnimateStatChanges(KingdomStatsSnapshot before, KingdomStatsSnapshot after, StatModifier modifier)
        {
            yield return UpdateStatSlot(TreasurySlotView, before.Treasury, after.Treasury);
            yield return UpdateStatSlot(SentimentSlotView, before.PublicSentiment, after.PublicSentiment);
            yield return UpdateStatSlot(SecuritySlotView, before.Security, after.Security);
        }

        public override void Cancel()
        {
            TreasurySlotView.CancelUpdateRate();
            SentimentSlotView.CancelUpdateRate();
            SecuritySlotView.CancelUpdateRate();
        }

        private IEnumerator UpdateStatSlot(StatSlotView slotView, float before, float after)
        {
            var valueDiff = after - before;
            if (Mathf.Abs(valueDiff) <= Mathf.Epsilon)
            {
                yield break;
            }
            
            var isPositive = valueDiff > 0f;
            var valueRate = ToRate(after);

            if (isPositive)
            {
                yield return slotView.IncreaseRate(valueRate);
            }
            else
            {
                yield return slotView.DecreaseRate(valueRate);
            }
        }
        
        private static float ToRate(float value)
        {
            return value / KingdomStats.MaxValue;
        }
    }
}