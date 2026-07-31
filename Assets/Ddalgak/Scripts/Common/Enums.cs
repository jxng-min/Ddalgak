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
        WeekSettlement,
        ClearCheck,
        TurnEnd,
        GameOver,
        Clear
    }

    public enum EEventType
    {
        NormalChoice,
        ActionChoice,
        SuddenChoice
    }

    public enum EGameOverReason
    {
        None,
        TreasuryDepleted,
        PublicSentimentCollapsed,
        SecurityCollapsed,
        FatalEventFailure
    }
}
