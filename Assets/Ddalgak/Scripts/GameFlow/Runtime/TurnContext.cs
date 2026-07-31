using System;
using System.Collections.Generic;

namespace Ddalgak
{
    public sealed class TurnContext
    {
        private static readonly IReadOnlyList<ActionStepData> EmptyActionSteps = Array.Empty<ActionStepData>();

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
                    return Event.actionSteps is { Count: > 0 };
                }

                return SelectedChoice?.actionSteps is { Count: > 0 };
            }
        }

        public IReadOnlyList<ActionStepData> GetActionSteps()
        {
            if (Event == null)
            {
                return EmptyActionSteps;
            }

            if (Event.eventType == EEventType.SuddenChoice)
            {
                return Event.actionSteps ?? EmptyActionSteps;
            }

            return SelectedChoice?.actionSteps ?? EmptyActionSteps;
        }
    }
}
