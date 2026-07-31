using System.Collections.Generic;

namespace Ddalgak
{
    public static class GameEndingDataCatalog
    {
        private static readonly IReadOnlyList<EmergencyRecoveryData> EmergencyRecoveries =
            new List<EmergencyRecoveryData>
            {
                CreateEmergency("E01", "전리품으로 채운 금고", EKingdomStatType.Treasury,
                    EKingdomStatType.Security, 0.7f, new StatModifier(0, -5, -30),
                    "강한 군대를 이용해 인접한 영지를 공격하고 전리품을 확보합니다.",
                    "왕국군이 전리품을 가지고 돌아와 국고를 다시 채웠습니다.",
                    "원정군은 성과 없이 마지막 보급품까지 소모했습니다."),
                CreateEmergency("E02", "왕국 금붙이 모으기 운동", EKingdomStatType.Treasury,
                    EKingdomStatType.PublicSentiment, 0.8f, new StatModifier(0, -30, 0),
                    "백성들이 귀금속과 저축한 동전을 자발적으로 왕실에 내놓습니다.",
                    "모인 귀금속으로 재정 파탄을 가까스로 피했습니다.",
                    "모인 귀금속만으로 무너진 재정을 되돌리지 못했습니다."),
                CreateEmergency("E03", "왕궁 앞 강제 진압", EKingdomStatType.PublicSentiment,
                    EKingdomStatType.Security, 0.7f, new StatModifier(-5, 0, -30),
                    "충성스러운 군대에 왕궁으로 향하는 봉기를 진압하라고 명령합니다.",
                    "군대가 군중을 해산시키며 왕좌를 가까스로 지켰습니다.",
                    "진압군 일부가 명령을 거부하고 백성들의 편에 섰습니다."),
                CreateEmergency("E04", "왕실 긴급 지원금", EKingdomStatType.PublicSentiment,
                    EKingdomStatType.Treasury, 0.75f, new StatModifier(-30, 0, 0),
                    "왕실은 쌓아 둔 재화를 풀어 식량과 지원금을 전달합니다.",
                    "지원 물자가 전달되며 거리의 분노가 가라앉았습니다.",
                    "지원 물자가 사라졌다는 소문으로 분노가 더 커졌습니다."),
                CreateEmergency("E05", "왕실 용병대 소집", EKingdomStatType.Security,
                    EKingdomStatType.Treasury, 0.7f, new StatModifier(-30, 0, 0),
                    "남은 재산을 쏟아부어 대륙 각지의 용병단을 불러 모읍니다.",
                    "용병단이 성문과 성벽을 장악해 방어선을 복구했습니다.",
                    "용병들은 선금만 챙긴 채 왕국을 떠났습니다."),
                CreateEmergency("E06", "백성 자경단 결성", EKingdomStatType.Security,
                    EKingdomStatType.PublicSentiment, 0.8f, new StatModifier(0, -30, 0),
                    "백성들이 직접 무기를 들고 성문과 주요 길목으로 모여듭니다.",
                    "자경단이 적군의 진격을 늦추고 수도를 지켜냈습니다.",
                    "자경단은 끝까지 저항했지만 적군을 막지 못했습니다.")
            };

        private static readonly IReadOnlyList<GameOverPresentationData> GameOvers =
            new List<GameOverPresentationData>
            {
                new()
                {
                    reason = EGameOverReason.TreasuryDepleted,
                    title = "텅 빈 왕실 금고",
                    presentation = "왕실 금고와 왕좌에 압류 딱지가 붙고 채권자들이 왕관까지 가져갑니다.",
                    message = "왕실의 금고가 완전히 바닥났습니다. 왕국이 먼저 팔렸을 뿐입니다."
                },
                new()
                {
                    reason = EGameOverReason.PublicSentimentCollapsed,
                    title = "왕궁으로 향한 행진",
                    presentation = "분노한 백성들이 왕궁으로 몰려오고 경비병들마저 군중에 합류합니다.",
                    message = "명령을 들어 줄 사람은 남아 있지 않았습니다. 왕의 동상이 있던 자리에는 새로운 광장이 생겼습니다."
                },
                new()
                {
                    reason = EGameOverReason.SecurityCollapsed,
                    title = "활짝 열린 성문",
                    presentation = "무너진 성문을 지나 적군이 아무런 저항 없이 수도로 들어옵니다.",
                    message = "왕은 끝까지 왕좌를 지켰습니다. 도망칠 길을 찾지 못했기 때문입니다."
                }
            };

        public static EmergencyRecoveryData GetEmergency(EKingdomStatType collapsedStat,
                                                          EKingdomStatType resourceStat)
        {
            foreach (EmergencyRecoveryData data in EmergencyRecoveries)
            {
                if (data.collapsedStat == collapsedStat && data.resourceStat == resourceStat)
                {
                    return data;
                }
            }

            return null;
        }

        public static GameOverPresentationData GetGameOver(EGameOverReason reason)
        {
            foreach (GameOverPresentationData data in GameOvers)
            {
                if (data.reason == reason)
                {
                    return data;
                }
            }

            return new GameOverPresentationData
            {
                reason = reason,
                title = "통치 실패",
                message = reason.ToString()
            };
        }

        private static EmergencyRecoveryData CreateEmergency(string id,
                                                              string title,
                                                              EKingdomStatType collapsedStat,
                                                              EKingdomStatType resourceStat,
                                                              float probability,
                                                              StatModifier successCost,
                                                              string description,
                                                              string successText,
                                                              string failureText)
        {
            return new EmergencyRecoveryData
            {
                recoveryId = id,
                title = title,
                collapsedStat = collapsedStat,
                resourceStat = resourceStat,
                successProbability = probability,
                recoveryValue = 20,
                successCost = successCost,
                description = description,
                successText = successText,
                failureText = failureText
            };
        }
    }
}
