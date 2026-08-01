using System.Collections.Generic;
using JxModule.DataTable;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public sealed class ChoiceDataTableRow : DataTableRowBase
    {
        public Key inputKey;
        public string description;
        public string changePreview;
        public string baseResultId;
        public bool hasRandomResult;
        public float successProbability;
        public string randomSuccessResultId;
        public string randomFailureResultId;
        public string actionSuccessResultId;
        public string actionFailureResultId;
        public bool isFatalOnActionFailure;
        public List<string> buttonActionIds = new();

        public ChoiceData ToRuntimeData(IReadOnlyDictionary<string, ResultDataTableRow> results,
                                        IReadOnlyDictionary<string, ButtonAction> actions)
        {
            ResultDataTableRow baseResult = FindResult(results, baseResultId);
            ResultDataTableRow randomSuccess = FindResult(results, randomSuccessResultId);
            ResultDataTableRow randomFailure = FindResult(results, randomFailureResultId);
            ResultDataTableRow actionSuccess = FindResult(results, actionSuccessResultId);
            ResultDataTableRow actionFailure = FindResult(results, actionFailureResultId);

            ChoiceData result = new()
            {
                choiceId = rowID,
                inputKey = inputKey,
                description = description,
                changePreview = changePreview,
                baseModifier = baseResult?.ToModifier() ?? StatModifier.Zero,
                hasRandomResult = hasRandomResult,
                successProbability = successProbability,
                randomSuccessModifier = randomSuccess?.ToModifier() ?? StatModifier.Zero,
                randomFailureModifier = randomFailure?.ToModifier() ?? StatModifier.Zero,
                randomSuccessResultText = randomSuccess?.resultText,
                randomFailureResultText = randomFailure?.resultText,
                actionSuccessModifier = actionSuccess?.ToModifier() ?? StatModifier.Zero,
                actionFailureModifier = actionFailure?.ToModifier() ?? StatModifier.Zero,
                successResultText = actionSuccess?.resultText ?? baseResult?.resultText,
                failureResultText = actionFailure?.resultText,
                isFatalOnActionFailure = isFatalOnActionFailure
            };

            AddActions(result.buttonActions, actions);
            return result;
        }

        private void AddActions(ICollection<ButtonAction> target,
                                IReadOnlyDictionary<string, ButtonAction> actions)
        {
            if (buttonActionIds == null)
            {
                return;
            }

            foreach (string id in buttonActionIds)
            {
                if (actions.TryGetValue(id, out ButtonAction action))
                {
                    target.Add(action);
                }
                else
                {
                    Debug.LogWarning($"ButtonAction row not found: {id}");
                }
            }
        }

        private static ResultDataTableRow FindResult(
            IReadOnlyDictionary<string, ResultDataTableRow> results,
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            if (results.TryGetValue(id, out ResultDataTableRow result))
            {
                return result;
            }

            Debug.LogWarning($"Result row not found: {id}");
            return null;
        }
    }
}
