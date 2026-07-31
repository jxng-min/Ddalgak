namespace Ddalgak
{
    public sealed class GovernanceResultRecord
    {
        public bool IsClear { get; }
        public int ReignWeek { get; }
        public int ResolvedEventCount { get; }
        public KingdomStatsSnapshot FinalStats { get; }
        public int ActionSuccessCount { get; }
        public int ActionFailureCount { get; }
        public bool EmergencyOccurred { get; }
        public bool EmergencySucceeded { get; }
        public EGameOverReason GameOverReason { get; }

        public GovernanceResultRecord(bool isClear,
                                      int reignWeek,
                                      int resolvedEventCount,
                                      KingdomStatsSnapshot finalStats,
                                      int actionSuccessCount,
                                      int actionFailureCount,
                                      bool emergencyOccurred,
                                      bool emergencySucceeded,
                                      EGameOverReason gameOverReason)
        {
            IsClear = isClear;
            ReignWeek = reignWeek;
            ResolvedEventCount = resolvedEventCount;
            FinalStats = finalStats;
            ActionSuccessCount = actionSuccessCount;
            ActionFailureCount = actionFailureCount;
            EmergencyOccurred = emergencyOccurred;
            EmergencySucceeded = emergencySucceeded;
            GameOverReason = gameOverReason;
        }
    }
}
