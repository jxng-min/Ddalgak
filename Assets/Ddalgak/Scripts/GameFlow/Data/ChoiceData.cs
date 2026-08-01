using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    [Serializable]
    public sealed class ChoiceData
    {
        public string choiceId;
        public Key inputKey;
        public string description;
        public string changePreview;
        public StatModifier baseModifier;
        public bool hasRandomResult;
        [Range(0f, 1f)] public float successProbability = 0.5f;
        public StatModifier randomSuccessModifier;
        public StatModifier randomFailureModifier;
        public string randomSuccessResultText;
        public string randomFailureResultText;
        public StatModifier actionSuccessModifier;
        public StatModifier actionFailureModifier;
        public string successResultText;
        public string failureResultText;
        public bool isFatalOnActionFailure;
        public List<ButtonAction> buttonActions = new();
    }
}
