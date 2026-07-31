using System;

namespace Ddalgak
{
    [Serializable]
    public sealed class GameOverPresentationData
    {
        public EGameOverReason reason;
        public string title;
        public string presentation;
        public string message;
    }
}
