namespace Ddalgak
{
    public sealed class TurnResult
    {
        public StatModifier FinalModifier { get; }
        public string ResultText { get; }
        public bool IsFatalFailure { get; }

        public TurnResult(StatModifier finalModifier, string resultText, bool isFatalFailure)
        {
            FinalModifier = finalModifier;
            ResultText = resultText;
            IsFatalFailure = isFatalFailure;
        }
    }
}
