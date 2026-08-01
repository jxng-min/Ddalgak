using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    [Serializable]
    public sealed class ButtonActionSerializedData
    {
        public string actionID;
        public EButtonActionType actionType;
        public Key key;
        public float duration;
        public float holdDuration;
        public float addDuration;
        public int targetPressCount;
        public float timingSpeed;
        public float successRangeStart;
        public float successRangeEnd;
    }
}
