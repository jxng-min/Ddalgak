using System.Collections;
using UnityEngine;

namespace Ddalgak
{
    public abstract class EndingPresenterBase : MonoBehaviour
    {
        public abstract IEnumerator ShowEmergencyRecovery(EmergencyRecoveryData data);
        public abstract IEnumerator ShowEmergencyRecoveryResult(EmergencyRecoveryData data, bool succeeded);
        public abstract IEnumerator ShowGameOver(GameOverPresentationData data);
        public abstract IEnumerator ShowClear();
        public abstract IEnumerator ShowGovernanceResult(GovernanceResultRecord record);
        public abstract void Cancel();
    }
}