using System.Collections;
using UnityEngine;

namespace Ddalgak
{
    public abstract class ProgressPresenterBase : MonoBehaviour
    {
        public abstract IEnumerator ShowGameStart(GameRuntimeState runtimeState);
        public abstract IEnumerator ShowWeekStart(GameRuntimeState runtimeState);
        public abstract IEnumerator ShowWeekSettlement(GameRuntimeState runtimeState);
        public abstract void Cancel();
    }
}