using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public abstract class ActionSequenceRunnerBase : MonoBehaviour
    {
        public abstract IEnumerator Run(IReadOnlyList<ButtonAction> buttonActions,
                                        Action<ActionSequenceResult> onCompleted);
        public abstract void Cancel();
    }
}
