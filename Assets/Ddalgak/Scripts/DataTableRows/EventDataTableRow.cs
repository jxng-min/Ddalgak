using System.Collections.Generic;
using JxModule;
using JxModule.DataTable;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EventDataTableRow : DataTableRowBase
    {
        public string eventName;
        public string title;
        public string description;
        public Texture2D eventTexture;
        public int week;
        public EEventType eventType;
        public float weight;
        public bool isConditional;
        public EKingdomStatType conditionalStat;
        public int minTreasury;
        public int maxTreasury;
        public int minPublicSentiment;
        public int maxPublicSentiment;
        public int minSecurity;
        public int maxSecurity;
        public List<string> requiredEventIds = new();
        public List<string> choiceIds = new();
        public List<string> buttonActionIds = new();
        public string actionSuccessResultId;
        public string actionFailureResultId;
        public bool isFatalOnActionFailure;

        public EventData ToRuntimeData(IReadOnlyDictionary<string, ChoiceData> choices,
                                       IReadOnlyDictionary<string, ButtonAction> actions,
                                       IReadOnlyDictionary<string, ResultDataTableRow> results)
        {
            ResultDataTableRow actionSuccess = FindResult(results, actionSuccessResultId);
            ResultDataTableRow actionFailure = FindResult(results, actionFailureResultId);
            EventData result = new()
            {
                eventId = rowID,
                eventName = eventName,
                title = title,
                description = description,
                eventImage = eventTexture.ToSprite(),
                week = week,
                eventType = eventType,
                weight = weight,
                isConditional = isConditional,
                conditionalStat = conditionalStat,
                minTreasury = minTreasury,
                maxTreasury = maxTreasury,
                minPublicSentiment = minPublicSentiment,
                maxPublicSentiment = maxPublicSentiment,
                minSecurity = minSecurity,
                maxSecurity = maxSecurity,
                requiredEventIds = requiredEventIds ?? new List<string>(),
                actionSuccessModifier = actionSuccess?.ToModifier() ?? StatModifier.Zero,
                actionFailureModifier = actionFailure?.ToModifier() ?? StatModifier.Zero,
                successResultText = actionSuccess?.resultText,
                failureResultText = actionFailure?.resultText,
                isFatalOnActionFailure = isFatalOnActionFailure
            };

            AddChoices(result.choices, choices);
            AddActions(result.buttonActions, actions);
            return result;
        }

        private void AddChoices(ICollection<ChoiceData> target,
                                IReadOnlyDictionary<string, ChoiceData> choices)
        {
            if (choiceIds == null)
            {
                return;
            }

            foreach (string id in choiceIds)
            {
                if (choices.TryGetValue(id, out ChoiceData choice))
                {
                    target.Add(choice);
                }
                else
                {
                    Debug.LogWarning($"Choice row not found: {id}");
                }
            }
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
