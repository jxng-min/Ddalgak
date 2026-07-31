using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public sealed class DebugActionSequenceRunner : ActionSequenceRunnerBase
    {
        public EDebugOutcomeMode OutcomeMode { get; set; }

        public override IEnumerator Run(IReadOnlyList<ButtonAction> buttonActions,
                                        Action<ActionSequenceResult> onCompleted)
        {
            var stepCount = buttonActions?.Count ?? 0;

            bool succeeded = OutcomeMode != EDebugOutcomeMode.ForceFailure;
            Debug.Log($"[GameFlow] Debug action result: {(succeeded ? "Success" : "Failure")}. Steps: {stepCount}");

            onCompleted?.Invoke(succeeded
                ? ActionSequenceResult.Success(stepCount)
                : ActionSequenceResult.Failure(0, 0));
            
            yield break;
        }

        public override void Cancel()
        {
            
        }
    }
}
