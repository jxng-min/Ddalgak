using System.Collections.Generic;
using System.Linq;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EventSelector
    {
        private const int ConditionalActivationThreshold = 30;
        private const int ConditionalCriticalThreshold = 15;
        private const float ConditionalCriticalWeightMultiplier = 2f;
        private const int LargeRiskDecreaseThreshold = -10;

        public EventData Select(IReadOnlyList<EventData> allEvents,
                                GameRuntimeState state,
                                bool forceConditionalEvent)
        {
            if (allEvents == null)
            {
                return null;
            }

            var slotType = state.CurrentSlotType;
            var conditionalStat = FindConditionalStat(allEvents, state, slotType);
            var candidates = allEvents.Where(eventData => CanAppear(eventData, state, slotType)).Where(eventData => !eventData.isConditional || (conditionalStat.HasValue && eventData.conditionalStat == conditionalStat.Value)).ToList();

            if (forceConditionalEvent)
            {
                var conditionalCandidates = candidates.FindAll(eventData => eventData.isConditional);
                if (conditionalCandidates.Count > 0)
                {
                    return RandomUtility.GetWeightedRandom(conditionalCandidates,
                        eventData => GetEventWeight(eventData, state));
                }
            }

            var riskFilteredCandidates = FilterConsecutiveRisk(candidates, state);
            if (riskFilteredCandidates.Count > 0)
            {
                candidates = riskFilteredCandidates;
            }
            else if (candidates.Count > 0 && state.LastCompletedEvent != null)
            {
                Debug.Log("[GameFlow] Risk-stat restriction relaxed because no candidate remains.");
            }

            return RandomUtility.GetWeightedRandom(candidates,
                eventData => GetEventWeight(eventData, state));
        }

        private static bool CanAppear(EventData eventData,
                                      GameRuntimeState state,
                                      EEventType slotType)
        {
            if (eventData == null || eventData.eventType != slotType ||
                state.HasCompletedEvent(eventData.eventId))
            {
                return false;
            }

            if (eventData.isConditional && state.HasConditionalEventThisWeek)
            {
                return false;
            }

            var stats = state.Stats;
            if (stats.Treasury < eventData.minTreasury || stats.Treasury > eventData.maxTreasury ||
                stats.PublicSentiment < eventData.minPublicSentiment ||
                stats.PublicSentiment > eventData.maxPublicSentiment ||
                stats.Security < eventData.minSecurity || stats.Security > eventData.maxSecurity)
            {
                return false;
            }

            if (eventData.requiredEventIds == null)
            {
                return true;
            }

            foreach (var requiredEventId in eventData.requiredEventIds)
            {
                if (!state.HasCompletedEvent(requiredEventId))
                {
                    return false;
                }
            }

            return true;
        }

        private static EKingdomStatType? FindConditionalStat(IReadOnlyList<EventData> allEvents,
                                                              GameRuntimeState state,
                                                              EEventType slotType)
        {
            if (state.HasConditionalEventThisWeek)
            {
                return null;
            }

            List<EKingdomStatType> remainingStats = new()
            {
                EKingdomStatType.Treasury,
                EKingdomStatType.PublicSentiment,
                EKingdomStatType.Security
            };

            while (remainingStats.Count > 0)
            {
                var lowestValue = int.MaxValue;
                List<EKingdomStatType> lowestStats = new();
                foreach (EKingdomStatType statType in remainingStats)
                {
                    var value = GetStatValue(state, statType);
                    if (value < lowestValue)
                    {
                        lowestValue = value;
                        lowestStats.Clear();
                        lowestStats.Add(statType);
                    }
                    else if (value == lowestValue)
                    {
                        lowestStats.Add(statType);
                    }
                }

                if (lowestValue > ConditionalActivationThreshold)
                {
                    return null;
                }

                RandomUtility.Shuffle(lowestStats);
                foreach (var statType in lowestStats)
                {
                    if (HasConditionalCandidate(allEvents, state, slotType, statType))
                    {
                        return statType;
                    }

                    remainingStats.Remove(statType);
                }
            }

            return null;
        }

        private static bool HasConditionalCandidate(IReadOnlyList<EventData> allEvents,
                                                    GameRuntimeState state,
                                                    EEventType slotType,
                                                    EKingdomStatType statType)
        {
            return allEvents.Any(eventData => eventData is { isConditional: true } && 
                                              eventData.conditionalStat == statType && 
                                              CanAppear(eventData, state, slotType));
        }

        private static float GetEventWeight(EventData eventData, GameRuntimeState state)
        {
            if (eventData is not { isConditional: true })
            {
                return eventData?.weight ?? 0f;
            }

            return GetStatValue(state, eventData.conditionalStat) <= ConditionalCriticalThreshold
                ? eventData.weight * ConditionalCriticalWeightMultiplier
                : eventData.weight;
        }

        private static int GetStatValue(GameRuntimeState state, EKingdomStatType statType)
        {
            return statType switch
            {
                EKingdomStatType.Treasury => state.Stats.Treasury,
                EKingdomStatType.PublicSentiment => state.Stats.PublicSentiment,
                EKingdomStatType.Security => state.Stats.Security,
                _ => int.MaxValue
            };
        }

        private static List<EventData> FilterConsecutiveRisk(IReadOnlyList<EventData> candidates,
                                                             GameRuntimeState state)
        {
            var previousEvent = state.LastCompletedEvent;
            if (previousEvent == null)
            {
                return new List<EventData>(candidates);
            }

            var previousRiskStats = GetRiskStats(previousEvent);
            if (previousRiskStats.Count == 0)
            {
                return new List<EventData>(candidates);
            }

            var filtered = new List<EventData>();
            foreach (var candidate in candidates)
            {
                if (!SharesRiskStat(previousRiskStats, GetRiskStats(candidate)))
                {
                    filtered.Add(candidate);
                }
            }

            return filtered;
        }

        private static bool SharesRiskStat(IReadOnlyCollection<EKingdomStatType> left,
                                           IReadOnlyCollection<EKingdomStatType> right)
        {
            foreach (var leftStat in left)
            {
                foreach (var rightStat in right)
                {
                    if (leftStat == rightStat)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static HashSet<EKingdomStatType> GetRiskStats(EventData eventData)
        {
            var result = new HashSet<EKingdomStatType>();
            if (eventData == null)
            {
                return result;
            }

            AddRiskStats(result, eventData.actionSuccessModifier);
            AddRiskStats(result, eventData.actionFailureModifier);
            if (eventData.choices == null)
            {
                return result;
            }

            foreach (var choice in eventData.choices)
            {
                if (choice == null)
                {
                    continue;
                }

                AddRiskStats(result, choice.baseModifier);
                AddRiskStats(result, choice.randomSuccessModifier);
                AddRiskStats(result, choice.randomFailureModifier);
                AddRiskStats(result, choice.baseModifier + choice.actionSuccessModifier);
                AddRiskStats(result, choice.baseModifier + choice.actionFailureModifier);
            }

            return result;
        }

        private static void AddRiskStats(ISet<EKingdomStatType> result, StatModifier modifier)
        {
            if (modifier.treasury <= LargeRiskDecreaseThreshold)
            {
                result.Add(EKingdomStatType.Treasury);
            }

            if (modifier.publicSentiment <= LargeRiskDecreaseThreshold)
            {
                result.Add(EKingdomStatType.PublicSentiment);
            }

            if (modifier.security <= LargeRiskDecreaseThreshold)
            {
                result.Add(EKingdomStatType.Security);
            }
        }
    }
}
