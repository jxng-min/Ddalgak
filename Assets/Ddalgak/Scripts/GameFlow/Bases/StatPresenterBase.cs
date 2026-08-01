using System.Collections;
using UnityEngine;

namespace Ddalgak
{
    public abstract class StatPresenterBase : MonoBehaviour
    {
        public abstract void SetStats(KingdomStatsSnapshot stats);
        public abstract IEnumerator AnimateStatChanges(KingdomStatsSnapshot before,
                                                       KingdomStatsSnapshot after,
                                                       StatModifier modifier);
        public abstract void Cancel();
    }
}