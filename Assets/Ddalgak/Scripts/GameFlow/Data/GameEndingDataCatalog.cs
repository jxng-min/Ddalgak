using JxModule.DataTable;
using UnityEngine;

namespace Ddalgak
{
    public static class GameEndingDataCatalog
    {
        public static EmergencyRecoveryData GetEmergency(EKingdomStatType collapsedStat,
                                                          EKingdomStatType resourceStat)
        {
            EmergencyRecoveryDataTableRow row =
                DataTableManager.FindRow<EmergencyRecoveryDataTableRow>(candidate =>
                    candidate != null &&
                    candidate.isEnable &&
                    candidate.collapsedStat == collapsedStat &&
                    candidate.resourceStat == resourceStat);

            if (row != null)
            {
                return row.ToRuntimeData();
            }

            Debug.LogWarning(
                $"Emergency recovery row not found: {collapsedStat} / {resourceStat}");
            return null;
        }

        public static GameOverPresentationData GetGameOver(EGameOverReason reason)
        {
            EndingDataTableRow row = DataTableManager.FindRow<EndingDataTableRow>(candidate =>
                candidate != null &&
                candidate.isEnable &&
                candidate.gameOverReason == reason);

            if (row != null)
            {
                return row.ToRuntimeData();
            }

            Debug.LogWarning($"Game over ending row not found: {reason}");
            return new GameOverPresentationData
            {
                reason = reason,
                title = "통치 실패",
                message = reason.ToString()
            };
        }

        public static GameOverPresentationData GetClear()
        {
            EndingDataTableRow row = DataTableManager.FindRow<EndingDataTableRow>(candidate =>
                candidate != null &&
                candidate.isEnable &&
                candidate.gameOverReason == EGameOverReason.None);

            if (row != null)
            {
                return row.ToRuntimeData();
            }

            Debug.LogWarning("Clear ending row not found.");
            return new GameOverPresentationData
            {
                reason = EGameOverReason.None,
                title = "왕국은 오늘도 평화롭습니다",
                message = "수많은 위기 속에서도 왕국은 평화를 지켜 냈습니다."
            };
        }
    }
}
