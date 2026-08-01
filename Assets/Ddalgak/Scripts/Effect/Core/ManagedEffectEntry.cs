#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Ddalgak
{
    public sealed class ManagedEffectEntry
    {
        public MonoBehaviour targetObject;
        public ManagedEffectAttribute debugAttr;
        public Editor inspector;
        public bool isExpanded;
        public string assetPath;
    }
}
#endif