using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public class StatView : ViewBase
    {
        public Vector2 OriginAnchoredPosition { get; private set; }

        private void Awake()
        {
            OriginAnchoredPosition = RectTransform.anchoredPosition;
        }
    }
}