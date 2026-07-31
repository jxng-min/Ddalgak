using System;
using System.Collections.Generic;

namespace Ddalgak
{
    [Serializable]
    public sealed class EventData
    {
        public string eventId;
        public string eventName;
        public string title;
        public string description;
        public EEventType eventType;
        public int minProcedureLevel;
        public int maxProcedureLevel = 4;
        public float weight = 1f;
        public int minTreasury;
        public int maxTreasury = 100;
        public int minPublicSentiment;
        public int maxPublicSentiment = 100;
        public int minSecurity;
        public int maxSecurity = 100;
        public List<string> requiredEventIds = new();
        public List<ChoiceData> choices = new();
        public List<ActionStepData> actionSteps = new();
        public StatModifier actionSuccessModifier;
        public StatModifier actionFailureModifier;
        public string successResultText;
        public string failureResultText;
        public bool isFatalOnActionFailure;
    }
}
