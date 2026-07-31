using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Ddalgak
{
    public sealed class TurnResultCalculator
    {
        public TurnResult Calculate(TurnContext context, EDebugOutcomeMode probabilityMode)
        {
            return context.Event.eventType switch
            {
                EEventType.NormalChoice => CalculateNormalChoice(context.SelectedChoice, probabilityMode),
                EEventType.ActionChoice => CalculateActionChoice(context, probabilityMode),
                EEventType.SuddenChoice => CalculateSuddenChoice(context),
                _ => EmptyResult()
            };
        }

        public bool RollProbability(float probability, EDebugOutcomeMode mode)
        {
            return mode switch
            {
                EDebugOutcomeMode.ForceSuccess => true,
                EDebugOutcomeMode.ForceFailure => false,
                _ => Random.value < Mathf.Clamp01(probability)
            };
        }

        private TurnResult CalculateNormalChoice(ChoiceData choice, EDebugOutcomeMode mode)
        {
            if (choice == null)
            {
                return EmptyResult();
            }

            if (!choice.hasRandomResult)
            {
                return new TurnResult(choice.baseModifier, choice.successResultText, false);
            }

            var randomSucceeded = RollProbability(choice.successProbability, mode);
            return new TurnResult(choice.baseModifier + GetRandomModifier(choice, randomSucceeded),
                                  GetRandomResultText(choice, randomSucceeded),
                                  false,
                                  hasRandomResult: true,
                                  randomResultSucceeded: randomSucceeded);
        }

        private TurnResult CalculateActionChoice(TurnContext context, EDebugOutcomeMode mode)
        {
            var choice = context.SelectedChoice;
            if (choice == null)
            {
                return EmptyResult();
            }

            var actionSucceeded = context.ActionResult?.IsSuccess == true;
            var actionModifier = actionSucceeded ? choice.actionSuccessModifier : choice.actionFailureModifier;
            var finalModifier = choice.baseModifier + actionModifier;
            var resultText = actionSucceeded ? choice.successResultText : choice.failureResultText;
            var fatalFailure = !actionSucceeded && choice.isFatalOnActionFailure;

            if (fatalFailure || !choice.hasRandomResult)
            {
                return new TurnResult(finalModifier,
                                      resultText,
                                      fatalFailure,
                                      hasActionResult: true,
                                      actionSucceeded: actionSucceeded);
            }

            var randomSucceeded = RollProbability(choice.successProbability, mode);
            finalModifier += GetRandomModifier(choice, randomSucceeded);
            resultText = CombineResultText(resultText, GetRandomResultText(choice, randomSucceeded));

            return new TurnResult(finalModifier,
                                  resultText,
                                  false,
                                  hasActionResult: true,
                                  actionSucceeded: actionSucceeded,
                                  hasRandomResult: true,
                                  randomResultSucceeded: randomSucceeded);
        }

        private static TurnResult CalculateSuddenChoice(TurnContext context)
        {
            var succeeded = context.ActionResult?.IsSuccess == true;
            var eventData = context.Event;
            return new TurnResult(succeeded
                                      ? eventData.actionSuccessModifier
                                      : eventData.actionFailureModifier,
                                  succeeded ? eventData.successResultText : eventData.failureResultText,
                                  !succeeded && eventData.isFatalOnActionFailure,
                                  hasActionResult: true,
                                  actionSucceeded: succeeded);
        }

        private static StatModifier GetRandomModifier(ChoiceData choice, bool succeeded)
        {
            return succeeded ? choice.randomSuccessModifier : choice.randomFailureModifier;
        }

        private static string GetRandomResultText(ChoiceData choice, bool succeeded)
        {
            var randomText = succeeded ? choice.randomSuccessResultText
                                       : choice.randomFailureResultText;
            return !string.IsNullOrWhiteSpace(randomText)
                ? randomText
                : succeeded ? choice.successResultText : choice.failureResultText;
        }

        private static string CombineResultText(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first))
            {
                return second ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(second) ? first : $"{first}\n{second}";
        }

        private static TurnResult EmptyResult()
        {
            return new TurnResult(StatModifier.Zero, string.Empty, false);
        }
    }
}
