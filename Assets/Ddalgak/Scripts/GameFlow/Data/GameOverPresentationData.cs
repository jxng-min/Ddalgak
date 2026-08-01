using System;
using UnityEngine;

namespace Ddalgak
{
    [Serializable]
    public sealed class GameOverPresentationData
    {
        public EGameOverReason reason;
        public string title;
        public string presentation;
        public string message;
        public Sprite image;
    }
}
