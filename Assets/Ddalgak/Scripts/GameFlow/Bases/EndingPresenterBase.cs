using System.Collections;
using UnityEngine;

namespace Ddalgak
{
    public abstract class EndingPresenterBase : MonoBehaviour
    {
        public abstract IEnumerator ShowGovernanceResult(GovernanceResultRecord record);
        public abstract void Cancel();
    }
}
