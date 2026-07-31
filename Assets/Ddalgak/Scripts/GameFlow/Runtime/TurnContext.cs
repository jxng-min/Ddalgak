using System;
using System.Collections.Generic;

namespace Ddalgak
{
    public sealed class TurnContext
    {
        private static readonly IReadOnlyList<ButtonAction> EmptyButtonActions = Array.Empty<ButtonAction>();

        public EventData Event { get; set; }
        public ChoiceData SelectedChoice { get; set; }
        public ActionSequenceResult ActionResult { get; set; }
        public TurnResult Result { get; set; }
        public KingdomStatsSnapshot StatsBeforeUpdate { get; set; }
        public KingdomStatsSnapshot StatsAfterUpdate { get; set; }
        public EGameOverReason GameOverReason { get; set; }

        public bool HasChoice => Event != null &&
                                 (Event.eventType == EEventType.NormalChoice ||
                                  Event.eventType == EEventType.ActionChoice);

        public bool HasAction
        {
            get
            {
                if (Event == null)
                {
                    return false;
                }

                if (Event.eventType == EEventType.SuddenChoice)
                {
                    return Event.buttonActions is { Count: > 0 };
                }

                return SelectedChoice?.buttonActions is { Count: > 0 };
            }
        }

        public IReadOnlyList<ButtonAction> GetButtonActions()
        {
            if (Event == null)
            {
                return EmptyButtonActions;
            }

            if (Event.eventType == EEventType.SuddenChoice)
            {
                return Event.buttonActions ?? EmptyButtonActions;
            }

            return SelectedChoice?.buttonActions ?? EmptyButtonActions;
        }
    }
}
