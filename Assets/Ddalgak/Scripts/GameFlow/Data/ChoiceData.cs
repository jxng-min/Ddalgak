using System;
using System.Collections.Generic;

namespace Ddalgak
{
    [Serializable]
    public sealed class ChoiceData
    {
        public string choiceId;
        public EGameInputButton inputButton;
        public string description;
        public string changePreview;
        public StatModifier baseModifier;
        public StatModifier actionSuccessModifier;
        public StatModifier actionFailureModifier;
        public string successResultText;
        public string failureResultText;
        public bool isFatalOnActionFailure;
        public List<ActionStepData> actionSteps = new();
    }
}
