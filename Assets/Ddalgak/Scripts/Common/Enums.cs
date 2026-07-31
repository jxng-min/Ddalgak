namespace Ddalgak
{
    public enum EGameFlowState
    {
        None,
        Initializing,
        TurnStart,
        EventSelection,
        EventPresentation,
        Choice,
        Action,
        ResultCalculation,
        ResultPresentation,
        StatUpdate,
        GameOverCheck,
        ProcedureCheck,
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

    public enum EActionInputType
    {
        SinglePress,
        Hold,
        RepeatedPress,
        Timing
    }

    public enum EGameInputButton
    {
        Q,
        Space,
        P
    }
}
