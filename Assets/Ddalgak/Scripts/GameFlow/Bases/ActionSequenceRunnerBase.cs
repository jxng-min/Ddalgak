using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public abstract class ActionSequenceRunnerBase : MonoBehaviour
    {
        public abstract IEnumerator Run(IReadOnlyList<ActionStepData> actionSteps,
                                        Action<ActionSequenceResult> onCompleted);
        public abstract void Cancel();
    }
}
