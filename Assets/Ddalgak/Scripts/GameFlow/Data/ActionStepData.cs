using System;

namespace Ddalgak
{
    [Serializable]
    public sealed class ActionStepData
    {
        public string stepId;
        public EActionInputType inputType;
        public EGameInputButton inputButton;
        public float timeLimit;
        public float holdDuration;
        public int requiredPressCount;
        public string instructionText;
    }
}
