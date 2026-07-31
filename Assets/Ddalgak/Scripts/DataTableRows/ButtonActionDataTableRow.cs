using JxModule.DataTable;
using UnityEngine;

namespace Ddalgak
{
    public sealed class ButtonActionDataTableRow : DataTableRowBase
    {
        public EButtonActionType actionType;
        public KeyCode key;
        public float duration;
        public float holdDuration;
        public float addDuration;
        public int targetPressCount;
        public float timingSpeed;
        public float successRangeStart;
        public float successRangeEnd;

        public ButtonAction ToRuntimeData()
        {
            ButtonActionSerializedData serializedData = new()
            {
                actionID = rowID,
                actionType = actionType,
                key = key,
                duration = duration,
                holdDuration = holdDuration,
                addDuration = addDuration,
                targetPressCount = targetPressCount,
                timingSpeed = timingSpeed,
                successRangeStart = successRangeStart,
                successRangeEnd = successRangeEnd
            };

            return JsonUtility.FromJson<ButtonAction>(JsonUtility.ToJson(serializedData));
        }
    }
}
