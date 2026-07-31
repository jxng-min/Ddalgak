using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public sealed class DebugActionSequenceRunner : ActionSequenceRunnerBase
    {
        public override IEnumerator Run(IReadOnlyList<ButtonAction> buttonActions,
                                        Action<ActionSequenceResult> onCompleted)
        {
            var stepCount = buttonActions?.Count ?? 0;
            
            Debug.Log($"[GameFlow] Debug action sequence succeeds immediately. Steps: {stepCount}");
            
            onCompleted?.Invoke(ActionSequenceResult.Success(stepCount));
            
            yield break;
        }

        public override void Cancel()
        {
            
        }
    }
}
