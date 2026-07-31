using System.Collections.Generic;
using JxModule;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Ddalgak
{
    public sealed class WeekSlotPlanner
    {
        private const int EventsPerWeek = 4;

        public List<EEventType> Create(int week, GameRuntimeState state)
        {
            var slots = CreateBaseSlots(week);
            PreventThreeConsecutiveTypes(slots, week, state);
            return slots;
        }

        private static List<EEventType> CreateBaseSlots(int week)
        {
            switch (week)
            {
                case 1:
                {
                    var actionIndex = Random.Range(1, 3);
                    List<EEventType> slots = new(EventsPerWeek);
                    for (var i = 0; i < EventsPerWeek; i++)
                    {
                        slots.Add(i == actionIndex ? EEventType.ActionChoice : EEventType.NormalChoice);
                    }

                    return slots;
                }
                case 2:
                {
                    List<EEventType> remainingSlots = new()
                    {
                        EEventType.NormalChoice,
                        EEventType.NormalChoice,
                        EEventType.ActionChoice
                    };
                    
                    RandomUtility.Shuffle(remainingSlots);
                    
                    return new List<EEventType>
                    {
                        remainingSlots[0],
                        remainingSlots[1],
                        EEventType.SuddenChoice,
                        remainingSlots[2]
                    };
                }
                case 3:
                {
                    var normalFirst = Random.value < 0.5f;
                    return new List<EEventType>
                    {
                        normalFirst ? EEventType.NormalChoice : EEventType.ActionChoice,
                        normalFirst ? EEventType.ActionChoice : EEventType.NormalChoice,
                        EEventType.SuddenChoice,
                        EEventType.ActionChoice
                    };
                }
                default:
                    return new List<EEventType>();
            }
        }

        private static void PreventThreeConsecutiveTypes(List<EEventType> slots,
                                                         int week,
                                                         GameRuntimeState state)
        {
            if (slots.Count == 0 || state.LastCompletedEvent == null)
            {
                return;
            }

            var previousType = state.LastCompletedEvent.eventType;
            var consecutiveCount = state.ConsecutiveSameEventTypeCount;

            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i] == previousType)
                {
                    consecutiveCount++;
                }
                else
                {
                    previousType = slots[i];
                    consecutiveCount = 1;
                }

                if (consecutiveCount < 3 || IsFixedSlot(week, i))
                {
                    continue;
                }

                var swapIndex = FindSwappableDifferentSlot(slots, week, i, previousType);
                if (swapIndex < 0)
                {
                    Debug.Log("[GameFlow] Event-type restriction relaxed because no slot can be swapped.");
                    continue;
                }

                (slots[i], slots[swapIndex]) = (slots[swapIndex], slots[i]);
                previousType = slots[i];
                consecutiveCount = 1;
            }
        }

        private static int FindSwappableDifferentSlot(IReadOnlyList<EEventType> slots,
                                                      int week,
                                                      int currentIndex,
                                                      EEventType repeatedType)
        {
            for (var i = currentIndex + 1; i < slots.Count; i++)
            {
                if (!IsFixedSlot(week, i) && slots[i] != repeatedType)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsFixedSlot(int week, int slotIndex)
        {
            var suddenSlot = (week == 2 || week == 3) && slotIndex == 2;
            var finalActionSlot = week == 3 && slotIndex == 3;
            return suddenSlot || finalActionSlot;
        }
    }
}
