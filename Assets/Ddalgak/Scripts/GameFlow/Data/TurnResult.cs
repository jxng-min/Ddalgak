namespace Ddalgak
{
    public sealed class TurnResult
    {
        public StatModifier FinalModifier { get; }
        public string ResultText { get; }
        public bool IsFatalFailure { get; }
        public bool HasActionResult { get; }
        public bool ActionSucceeded { get; }
        public bool HasRandomResult { get; }
        public bool RandomResultSucceeded { get; }

        public TurnResult(StatModifier finalModifier,
                          string resultText,
                          bool isFatalFailure,
                          bool hasActionResult = false,
                          bool actionSucceeded = false,
                          bool hasRandomResult = false,
                          bool randomResultSucceeded = false)
        {
            FinalModifier = finalModifier;
            ResultText = resultText;
            IsFatalFailure = isFatalFailure;
            HasActionResult = hasActionResult;
            ActionSucceeded = actionSucceeded;
            HasRandomResult = hasRandomResult;
            RandomResultSucceeded = randomResultSucceeded;
        }
    }
}
