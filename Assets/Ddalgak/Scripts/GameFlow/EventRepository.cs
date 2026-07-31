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

            for (int i = 1; i <= 4; i++)
            {
                events.Add(CreateAdditionalNormalEvent(i));
            }

            for (int i = 1; i <= 3; i++)
            {
                events.Add(CreateAdditionalActionEvent(i));
            }

            events.Add(CreateAdditionalSuddenEvent());
            events.Add(CreateRoyalDebtEvent());
            events.Add(CreateProtestEvent());
            events.Add(CreateBorderPostEvent());
        }

        private static EventData CreateRoyalDebtEvent()
        {
            return new EventData
            {
                eventId = "B08",
                eventName = "Royal Debt Repayment Demand",
                title = "왕실 채무 상환 독촉",
                description = "왕실이 빌린 돈의 상환 기한이 다가왔습니다. 채권자들은 오늘 안으로 빚을 갚지 않으면 왕실의 재산을 압류하겠다고 통보했습니다.",
                eventType = EEventType.NormalChoice,
                weight = 1f,
                isConditional = true,
                conditionalStat = EKingdomStatType.Treasury,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "B08_Q",
                        inputKey = KeyCode.Q,
                        description = "백성에게 특별세를 걷는다.",
                        changePreview = "국고 +20 / 민심 -15",
                        baseModifier = new StatModifier(20, -15, 0),
                        successResultText = "백성들에게 급하게 특별세를 거두어 채무 일부를 상환했습니다."
                    },
                    new()
                    {
                        choiceId = "B08_SPACE",
                        inputKey = KeyCode.Space,
                        description = "군수 물자를 매각한다.",
                        changePreview = "국고 +15 / 안보 -15",
                        baseModifier = new StatModifier(15, 0, -15),
                        successResultText = "비축해 둔 무기와 군량을 주변 왕국에 판매했습니다."
                    },
                    new()
                    {
                        choiceId = "B08_P",
                        inputKey = KeyCode.P,
                        description = "상환 기한을 미뤄 달라고 협상한다.",
                        changePreview = "성공 45% / 실패 55%",
                        hasRandomResult = true,
                        successProbability = 0.45f,
                        randomSuccessModifier = new StatModifier(-10, -5, -5),
                        randomFailureModifier = new StatModifier(-25, -10, 0),
                        successResultText = "채권자들은 당장의 압류를 미루는 대신 다른 것들을 요구했습니다.",
                        failureResultText = "설득에 실패했고 채권자들이 결국 국고를 거의 털어갔습니다."
                    }
                }
            };
        }

        private static EventData CreateProtestEvent()
        {
            return new EventData
            {
                eventId = "B09",
                eventName = "Protesters in the Square",
                title = "광장에 모인 시위대",
                description = "왕궁 앞 광장에 분노한 백성들이 모였습니다. 시위대는 최근 왕실의 정책에 항의하며 즉각적인 대책을 요구하고 있습니다.",
                eventType = EEventType.NormalChoice,
                weight = 1f,
                isConditional = true,
                conditionalStat = EKingdomStatType.PublicSentiment,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "B09_Q",
                        inputKey = KeyCode.Q,
                        description = "왕실 지원금을 지급한다.",
                        changePreview = "국고 -15 / 민심 +20",
                        baseModifier = new StatModifier(-15, 20, 0),
                        successResultText = "왕실은 백성들에게 긴급 지원금을 나누어 주고 불만을 달랬습니다."
                    },
                    new()
                    {
                        choiceId = "B09_SPACE",
                        inputKey = KeyCode.Space,
                        description = "경비대를 동원해 해산시킨다.",
                        changePreview = "민심 +5 / 안보 -10",
                        baseModifier = new StatModifier(0, 5, -10),
                        successResultText = "경비대가 광장을 비웠지만 백성들의 불만은 완전히 사라지지 않았습니다."
                    },
                    new()
                    {
                        choiceId = "B09_P",
                        inputKey = KeyCode.P,
                        description = "광장에 나가 직접 사과한다.",
                        changePreview = "성공 50% / 실패 50%",
                        hasRandomResult = true,
                        successProbability = 0.5f,
                        randomSuccessModifier = new StatModifier(0, 15, -5),
                        randomFailureModifier = new StatModifier(0, -25, -5),
                        successResultText = "왕실은 백성들 앞에서 잘못을 인정했고 국민들은 그 말을 믿는 듯합니다.",
                        failureResultText = "국민들이 사과를 납득하지 않았고 불만이 더욱 커졌습니다."
                    }
                }
            };
        }

        private static EventData CreateBorderPostEvent()
        {
            return new EventData
            {
                eventId = "B10",
                eventName = "Crumbling Border Post",
                title = "무너져 가는 국경 초소",
                description = "국경을 지키는 병사들이 장비와 인력 부족을 호소하고 있습니다. 이대로 방치하면 주변 세력이 왕국의 약해진 방어선을 눈치챌 수 있습니다.",
                eventType = EEventType.NormalChoice,
                weight = 1f,
                isConditional = true,
                conditionalStat = EKingdomStatType.Security,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "B10_Q",
                        inputKey = KeyCode.Q,
                        description = "대규모 지원 병력을 보낸다.",
                        changePreview = "국고 -15 / 안보 +20",
                        baseModifier = new StatModifier(-15, 0, 20),
                        successResultText = "왕실은 많은 비용을 들여 병력과 장비를 국경으로 보냈습니다."
                    },
                    new()
                    {
                        choiceId = "B10_SPACE",
                        inputKey = KeyCode.Space,
                        description = "백성들을 민병대로 징집한다.",
                        changePreview = "국고 -5 / 민심 -15 / 안보 +15",
                        baseModifier = new StatModifier(-5, -15, 15),
                        successResultText = "급하게 병력을 확보했지만 강제 징집에 대한 백성들의 불만이 커졌습니다."
                    },
                    new()
                    {
                        choiceId = "B10_P",
                        inputKey = KeyCode.P,
                        description = "초소를 줄이고 핵심 지역만 지킨다.",
                        changePreview = "민심 -5 / 안보 -10",
                        baseModifier = new StatModifier(0, -5, -10),
                        successResultText = "방어선을 축소해 남은 병력을 중요한 지역에 집중했습니다."
                    }
                }
            };
        }

        private static EventData CreateAdditionalNormalEvent(int index)
        {
            return new EventData
            {
                eventId = $"test_normal_{index}",
                eventName = $"Test Normal {index}",
                title = $"테스트 기본 선택 이벤트 {index}",
                description = "주차 슬롯과 중복 방지 흐름을 확인하기 위한 테스트 이벤트입니다.",
                eventType = EEventType.NormalChoice,
                weight = 1f,
                choices = CreateTestChoices($"normal_{index}", false)
            };
        }

        private static EventData CreateAdditionalActionEvent(int index)
        {
            return new EventData
            {
                eventId = $"test_action_{index}",
                eventName = $"Test Action {index}",
                title = $"테스트 실행형 이벤트 {index}",
                description = "실행형 슬롯과 액션 성공 흐름을 확인하기 위한 테스트 이벤트입니다.",
                eventType = EEventType.ActionChoice,
                weight = 1f,
                choices = CreateTestChoices($"action_{index}", true)
            };
        }

        private static EventData CreateAdditionalSuddenEvent()
        {
            return new EventData
            {
                eventId = "test_sudden_1",
                eventName = "Test Sudden 1",
                title = "테스트 돌발 액션 이벤트",
                description = "돌발 액션 슬롯 위치를 확인하기 위한 테스트 이벤트입니다.",
                eventType = EEventType.SuddenChoice,
                weight = 1f,
                buttonActions = new List<ButtonAction> { new() },
                actionSuccessModifier = StatModifier.Zero,
                actionFailureModifier = StatModifier.Zero,
                successResultText = "돌발 액션 테스트에 성공했습니다.",
                failureResultText = "돌발 액션 테스트에 실패했습니다."
            };
        }

        private static List<ChoiceData> CreateTestChoices(string idPrefix, bool hasAction)
        {
            return new List<ChoiceData>
            {
                CreateTestChoice($"{idPrefix}_q", KeyCode.Q, "첫 번째 테스트 선택지", hasAction),
                CreateTestChoice($"{idPrefix}_space", KeyCode.Space, "두 번째 테스트 선택지", hasAction),
                CreateTestChoice($"{idPrefix}_p", KeyCode.P, "세 번째 테스트 선택지", hasAction)
            };
        }

        private static ChoiceData CreateTestChoice(string choiceId,
                                                   KeyCode inputKey,
                                                   string description,
                                                   bool hasAction)
        {
            StatModifier modifier = inputKey switch
            {
                KeyCode.Q => new StatModifier(-15, 0, 0),
                KeyCode.Space => new StatModifier(0, -15, 0),
                KeyCode.P => new StatModifier(0, 0, -15),
                _ => StatModifier.Zero
            };

            string changePreview = inputKey switch
            {
                KeyCode.Q => "국고 -15",
                KeyCode.Space => "민심 -15",
                KeyCode.P => "안보 -15",
                _ => "변화 없음"
            };

            ChoiceData choice = new()
            {
                choiceId = choiceId,
                inputKey = inputKey,
                description = description,
                changePreview = changePreview,
                baseModifier = modifier,
                actionSuccessModifier = StatModifier.Zero,
                actionFailureModifier = StatModifier.Zero,
                successResultText = "선택한 왕국 수치가 15 감소했습니다.",
                failureResultText = "테스트 액션이 실패했습니다."
            };

            if (hasAction)
            {
                choice.buttonActions.Add(new ButtonAction());
            }

            return choice;
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
                weight = 1f,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "distribute_food",
                        inputKey = KeyCode.Q,
                        description = "국고를 풀어 식량을 배급한다.",
                        changePreview = "국고 ▼ / 민심 ▲",
                        baseModifier = new StatModifier(-15, 15, 0),
                        successResultText = "왕실 창고가 열렸고 백성들은 한숨을 돌렸습니다."
                    },
                    new()
                    {
                        choiceId = "reduce_tax",
                        inputKey = KeyCode.Space,
                        description = "세금을 감면한다.",
                        changePreview = "국고 ▼ / 민심 ▲",
                        baseModifier = new StatModifier(-10, 10, 0),
                        successResultText = "세금 부담이 줄어 백성들의 불만이 잦아들었습니다."
                    },
                    new()
                    {
                        choiceId = "order_saving",
                        inputKey = KeyCode.P,
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
                weight = 1f,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "send_knights",
                        inputKey = KeyCode.Q,
                        description = "기사단을 파견한다.",
                        changePreview = "국고 ▼ / 안보 ▲",
                        baseModifier = new StatModifier(-10, 0, 10),
                        actionSuccessModifier = new StatModifier(0, 0, 5),
                        actionFailureModifier = new StatModifier(0, -5, -5),
                        successResultText = "기사단이 제시간에 도착해 도적을 몰아냈습니다.",
                        failureResultText = "승인 절차가 늦어 도적들이 창고를 털고 달아났습니다.",
                        buttonActions = new List<ButtonAction> { new() }
                    },
                    new()
                    {
                        choiceId = "hire_mercenaries",
                        inputKey = KeyCode.Space,
                        description = "용병을 고용한다.",
                        changePreview = "국고 ▼▼ / 안보 ▲",
                        baseModifier = new StatModifier(-15, 0, 10),
                        actionSuccessModifier = new StatModifier(0, 0, 5),
                        actionFailureModifier = new StatModifier(-5, 0, -5),
                        successResultText = "용병들이 도적을 빠르게 제압했습니다.",
                        failureResultText = "계약이 꼬여 비용만 늘고 대응은 늦어졌습니다.",
                        buttonActions = new List<ButtonAction> { new() }
                    },
                    new()
                    {
                        choiceId = "village_guard",
                        inputKey = KeyCode.P,
                        description = "마을 경비대에 맡긴다.",
                        changePreview = "민심 ▼ / 안보 ▲",
                        baseModifier = new StatModifier(0, -5, 5),
                        actionSuccessModifier = new StatModifier(0, 5, 5),
                        actionFailureModifier = new StatModifier(0, -5, -10),
                        successResultText = "마을 경비대가 힘을 합쳐 도적을 막아냈습니다.",
                        failureResultText = "경비대의 대응이 무너져 마을이 큰 피해를 입었습니다.",
                        buttonActions = new List<ButtonAction> { new() }
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
                weight = 1f,
                buttonActions = new List<ButtonAction> { new() },
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
                weight = 1f,
                choices = new List<ChoiceData>
                {
                    new()
                    {
                        choiceId = "large_trade",
                        inputKey = KeyCode.Q,
                        description = "대규모 교역을 승인한다.",
                        changePreview = "국고 ▲ / 안보 ▼",
                        baseModifier = new StatModifier(15, 0, -10),
                        successResultText = "값비싼 물품이 들어왔지만 국경 검문이 느슨해졌습니다."
                    },
                    new()
                    {
                        choiceId = "small_trade",
                        inputKey = KeyCode.Space,
                        description = "소규모 교역만 허가한다.",
                        changePreview = "국고 ▲",
                        baseModifier = new StatModifier(5, 0, 0),
                        successResultText = "작은 이익을 얻으며 위험을 피했습니다."
                    },
                    new()
                    {
                        choiceId = "reject_trade",
                        inputKey = KeyCode.P,
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
