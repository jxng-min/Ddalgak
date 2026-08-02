namespace Ddalgak
{
    public sealed class TurnResult
    {
        public StatModifier FinalModifier { get; }
        public string ResultText { get; }
        public string ChangePreview { get; }
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
                          bool randomResultSucceeded = false,
                          string changePreview = null)
        {
            FinalModifier = finalModifier;
            ResultText = resultText;
            ChangePreview = changePreview;
            IsFatalFailure = isFatalFailure;
            HasActionResult = hasActionResult;
            ActionSucceeded = actionSucceeded;
            HasRandomResult = hasRandomResult;
            RandomResultSucceeded = randomResultSucceeded;
        }
    }
}
