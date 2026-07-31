namespace Ddalgak
{
    public enum EGameFlowState
    {
        None,
        Initializing,
        WeekStart,
        TurnStart,
        EventSelection,
        EventPresentation,
        Choice,
        Action,
        ResultCalculation,
        ResultPresentation,
        StatUpdate,
        GameOverCheck,
        EmergencyRecovery,
        WeekSettlement,
        ClearCheck,
        TurnEnd,
        GameOver,
        Clear,
        GovernanceResult
    }

    public enum EEventType
    {
        NormalChoice,
        ActionChoice,
        SuddenChoice
    }

    public enum EKingdomStatType
    {
        Treasury,
        PublicSentiment,
        Security
    }

    public enum EGameOverReason
    {
        None,
        TreasuryDepleted,
        PublicSentimentCollapsed,
        SecurityCollapsed,
        FatalEventFailure
    }

    public enum EDebugOutcomeMode
    {
        Default,
        ForceSuccess,
        ForceFailure
    }
    
    public enum EButtonActionType
    {
        None,
        SinglePress,
        Hold,
        RapidPress,
        Timing,
    }
}
