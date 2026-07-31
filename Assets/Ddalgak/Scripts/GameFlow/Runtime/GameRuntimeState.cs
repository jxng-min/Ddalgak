using System.Collections.Generic;

namespace Ddalgak
{
    public sealed class GameRuntimeState
    {
        private const int RecentEventCapacity = 3;

        private readonly Queue<string> _recentEventIds = new();
        private readonly HashSet<string> _completedEventIds = new();

        public KingdomStats Stats { get; } = new();
        public int ProcessedEventCount { get; private set; }
        public int ProcedureLevel { get; private set; }
        public bool IsGameFinished { get; private set; }
        public EGameOverReason GameOverReason { get; private set; }

        public IReadOnlyCollection<string> RecentEventIds => _recentEventIds;
        public IReadOnlyCollection<string> CompletedEventIds => _completedEventIds;

        public void Initialize()
        {
            Stats.Initialize();
            ProcessedEventCount = 0;
            ProcedureLevel = 0;
            IsGameFinished = false;
            GameOverReason = EGameOverReason.None;
            _recentEventIds.Clear();
            _completedEventIds.Clear();
        }

        public void CompleteEvent(string eventId)
        {
            ProcessedEventCount++;

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            _completedEventIds.Add(eventId);
            _recentEventIds.Enqueue(eventId);

            while (_recentEventIds.Count > RecentEventCapacity)
            {
                _recentEventIds.Dequeue();
            }
        }

        public void SetProcedureLevel(int level) => ProcedureLevel = level;
        public bool HasCompletedEvent(string eventId) => _completedEventIds.Contains(eventId);
        public bool IsRecentEvent(string eventId) => _recentEventIds.Contains(eventId);

        public void SetGameOver(EGameOverReason reason)
        {
            IsGameFinished = true;
            GameOverReason = reason;
        }

        public void SetClear()
        {
            IsGameFinished = true;
        }
    }
}
