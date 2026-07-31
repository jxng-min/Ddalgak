using System.Collections.Generic;

namespace Ddalgak
{
    public sealed class GameRuntimeState
    {
        private readonly HashSet<string> _completedEventIds = new();
        private readonly List<EEventType> _currentWeekSlots = new();

        public KingdomStats Stats { get; } = new();
        public int CurrentWeek { get; private set; }
        public int CurrentSlotIndex { get; private set; }
        public int ProcessedEventCount { get; private set; }
        public bool IsGameFinished { get; private set; }
        public EGameOverReason GameOverReason { get; private set; }

        public int EventsCompletedThisWeek => CurrentSlotIndex;
        public bool IsWeekCompleted => CurrentSlotIndex >= _currentWeekSlots.Count;
        public IReadOnlyList<EEventType> CurrentWeekSlots => _currentWeekSlots;
        public IReadOnlyCollection<string> CompletedEventIds => _completedEventIds;

        public EEventType CurrentSlotType
        {
            get
            {
                if (CurrentSlotIndex < 0 || CurrentSlotIndex >= _currentWeekSlots.Count)
                {
                    return default;
                }

                return _currentWeekSlots[CurrentSlotIndex];
            }
        }

        public void Initialize()
        {
            Stats.Initialize();
            CurrentWeek = 0;
            CurrentSlotIndex = 0;
            ProcessedEventCount = 0;
            IsGameFinished = false;
            GameOverReason = EGameOverReason.None;
            _currentWeekSlots.Clear();
            _completedEventIds.Clear();
        }

        public void StartWeek(int week, IReadOnlyList<EEventType> slots)
        {
            CurrentWeek = week;
            CurrentSlotIndex = 0;
            _currentWeekSlots.Clear();

            if (slots == null)
            {
                return;
            }

            foreach (EEventType slot in slots)
            {
                _currentWeekSlots.Add(slot);
            }
        }

        public void CompleteEvent(EventData eventData)
        {
            ProcessedEventCount++;
            CurrentSlotIndex++;

            if (eventData == null || string.IsNullOrWhiteSpace(eventData.eventId))
            {
                return;
            }

            _completedEventIds.Add(eventData.eventId);
        }

        public bool HasCompletedEvent(string eventId)
        {
            return _completedEventIds.Contains(eventId);
        }

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
