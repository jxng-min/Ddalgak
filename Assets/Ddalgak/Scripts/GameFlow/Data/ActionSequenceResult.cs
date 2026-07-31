namespace Ddalgak
{
    public sealed class ActionSequenceResult
    {
        public bool IsSuccess { get; }
        public int CompletedStepCount { get; }
        public int FailedStepIndex { get; }

        private ActionSequenceResult(bool isSuccess, int completedStepCount, int failedStepIndex)
        {
            IsSuccess = isSuccess;
            CompletedStepCount = completedStepCount;
            FailedStepIndex = failedStepIndex;
        }

        public static ActionSequenceResult Success(int completedStepCount)
        {
            return new ActionSequenceResult(true, completedStepCount, -1);
        }

        public static ActionSequenceResult Failure(int completedStepCount, int failedStepIndex)
        {
            return new ActionSequenceResult(false, completedStepCount, failedStepIndex);
        }
    }
}
