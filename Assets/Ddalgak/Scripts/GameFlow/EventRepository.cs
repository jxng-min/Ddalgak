using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EventRepository : EventRepositoryBase
    {
        [SerializeField] private List<EventData> events = new();

        public override IReadOnlyList<EventData> GetAllEvents()
        {
            return events;
        }

        [ContextMenu("Create Test Events")]
        private void CreateTestEvents()
        {
            events = new List<EventData>
            {
                CreateHarvestEvent(),
                CreateBanditEvent(),
                CreateAssassinEvent(),
                CreateMerchantEvent()
            };
        }

        private static EventData CreateHarvestEvent()
        {
            return new EventData
            {
                eventId = "harvest_shortage",
                eventName = "Harvest Shortage",
                title = "흉년이 찾아왔습니다",
                description = "올해 수확량이 크게 줄었습니다. 백성들이 왕의 결정을 기다립니다.",
                eventType = EEventType.NormalChoice,
                minProcedureLevel = 0,
                maxProcedureLevel = 4,
                weight = 1f,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "distribute_food",
                        inputButton = EGameInputButton.Q,
                        description = "국고를 풀어 식량을 배급한다.",
                        changePreview = "국고 ▼ / 민심 ▲",
                        baseModifier = new StatModifier(-15, 15, 0),
                        successResultText = "왕실 창고가 열렸고 백성들은 한숨을 돌렸습니다."
                    },
                    new()
                    {
                        choiceId = "reduce_tax",
                        inputButton = EGameInputButton.Space,
                        description = "세금을 감면한다.",
                        changePreview = "국고 ▼ / 민심 ▲",
                        baseModifier = new StatModifier(-10, 10, 0),
                        successResultText = "세금 부담이 줄어 백성들의 불만이 잦아들었습니다."
                    },
                    new()
                    {
                        choiceId = "order_saving",
                        inputButton = EGameInputButton.P,
                        description = "백성들에게 절약을 명한다.",
                        changePreview = "민심 ▼",
                        baseModifier = new StatModifier(0, -15, 0),
                        successResultText = "국고는 지켰지만 백성들의 원망이 커졌습니다."
                    }
                }
            };
        }

        private static EventData CreateBanditEvent()
        {
            return new EventData
            {
                eventId = "northern_bandits",
                eventName = "Northern Bandits",
                title = "북쪽 마을에 도적이 나타났습니다",
                description = "도적 떼가 창고를 노리고 있습니다. 병력을 보내야 합니다.",
                eventType = EEventType.ActionChoice,
                minProcedureLevel = 0,
                maxProcedureLevel = 4,
                weight = 1f,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "send_knights",
                        inputButton = EGameInputButton.Q,
                        description = "기사단을 파견한다.",
                        changePreview = "국고 ▼ / 안보 ▲",
                        baseModifier = new StatModifier(-10, 0, 10),
                        actionSuccessModifier = new StatModifier(0, 0, 5),
                        actionFailureModifier = new StatModifier(0, -5, -5),
                        successResultText = "기사단이 제시간에 도착해 도적을 몰아냈습니다.",
                        failureResultText = "승인 절차가 늦어 도적들이 창고를 털고 달아났습니다.",
                        actionSteps = new List<ActionStepData>
                        {
                            new()
                            {
                                stepId = "sign_order",
                                inputType = EActionInputType.Hold,
                                inputButton = EGameInputButton.Q,
                                timeLimit = 3f,
                                holdDuration = 1.5f,
                                instructionText = "Q를 길게 눌러 출동 명령서에 서명하십시오."
                            }
                        }
                    },
                    new()
                    {
                        choiceId = "hire_mercenaries",
                        inputButton = EGameInputButton.Space,
                        description = "용병을 고용한다.",
                        changePreview = "국고 ▼▼ / 안보 ▲",
                        baseModifier = new StatModifier(-15, 0, 10),
                        actionSuccessModifier = new StatModifier(0, 0, 5),
                        actionFailureModifier = new StatModifier(-5, 0, -5),
                        successResultText = "용병들이 도적을 빠르게 제압했습니다.",
                        failureResultText = "계약이 꼬여 비용만 늘고 대응은 늦어졌습니다.",
                        actionSteps = new List<ActionStepData>
                        {
                            new()
                            {
                                stepId = "approve_contract",
                                inputType = EActionInputType.SinglePress,
                                inputButton = EGameInputButton.Space,
                                timeLimit = 2f,
                                instructionText = "Space를 눌러 용병 계약을 승인하십시오."
                            }
                        }
                    },
                    new()
                    {
                        choiceId = "village_guard",
                        inputButton = EGameInputButton.P,
                        description = "마을 경비대에 맡긴다.",
                        changePreview = "민심 ▼ / 안보 ▲",
                        baseModifier = new StatModifier(0, -5, 5),
                        actionSuccessModifier = new StatModifier(0, 5, 5),
                        actionFailureModifier = new StatModifier(0, -5, -10),
                        successResultText = "마을 경비대가 힘을 합쳐 도적을 막아냈습니다.",
                        failureResultText = "경비대의 대응이 무너져 마을이 큰 피해를 입었습니다.",
                        actionSteps = new List<ActionStepData>
                        {
                            new()
                            {
                                stepId = "sound_alarm",
                                inputType = EActionInputType.RepeatedPress,
                                inputButton = EGameInputButton.P,
                                timeLimit = 3f,
                                requiredPressCount = 5,
                                instructionText = "P를 연타해 비상 종을 울리십시오."
                            }
                        }
                    }
                }
            };
        }

        private static EventData CreateAssassinEvent()
        {
            return new EventData
            {
                eventId = "throne_assassin",
                eventName = "Throne Assassin",
                title = "암살자가 나타났습니다!",
                description = "왕좌 뒤에서 암살자가 뛰쳐나왔습니다. 즉시 대응하십시오.",
                eventType = EEventType.SuddenChoice,
                minProcedureLevel = 0,
                maxProcedureLevel = 4,
                weight = 1f,
                actionSteps = new List<ActionStepData>
                {
                    new()
                    {
                        stepId = "stop_assassin",
                        inputType = EActionInputType.RepeatedPress,
                        inputButton = EGameInputButton.Space,
                        timeLimit = 3f,
                        requiredPressCount = 8,
                        instructionText = "Space를 빠르게 연타해 암살자를 막으십시오."
                    }
                },
                actionSuccessModifier = new StatModifier(0, 0, -5),
                actionFailureModifier = new StatModifier(0, -10, -20),
                successResultText = "근위대가 암살자를 제압했습니다.",
                failureResultText = "왕이 부상을 입고 왕궁의 경비 체계가 흔들렸습니다.",
                isFatalOnActionFailure = false
            };
        }

        private static EventData CreateMerchantEvent()
        {
            return new EventData
            {
                eventId = "foreign_merchant",
                eventName = "Foreign Merchant",
                title = "이국의 상인이 찾아왔습니다",
                description = "상인이 새로운 교역 계약을 제안했습니다.",
                eventType = EEventType.NormalChoice,
                minProcedureLevel = 0,
                maxProcedureLevel = 4,
                weight = 1f,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "large_trade",
                        inputButton = EGameInputButton.Q,
                        description = "대규모 교역을 승인한다.",
                        changePreview = "국고 ▲ / 안보 ▼",
                        baseModifier = new StatModifier(15, 0, -10),
                        successResultText = "값비싼 물품이 들어왔지만 국경 검문이 느슨해졌습니다."
                    },
                    new()
                    {
                        choiceId = "small_trade",
                        inputButton = EGameInputButton.Space,
                        description = "소규모 교역만 허가한다.",
                        changePreview = "국고 ▲",
                        baseModifier = new StatModifier(5, 0, 0),
                        successResultText = "작은 이익을 얻으며 위험을 피했습니다."
                    },
                    new()
                    {
                        choiceId = "reject_trade",
                        inputButton = EGameInputButton.P,
                        description = "교역을 거절한다.",
                        changePreview = "민심 ▼ / 안보 ▲",
                        baseModifier = new StatModifier(0, -5, 5),
                        successResultText = "상인은 떠났고 성문 경계는 한층 강화되었습니다."
                    }
                }
            };
        }
    }
}
