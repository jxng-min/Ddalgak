using JxModule;
using JxModule.DataTable;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EmergencyRecoveryDataTableRow : DataTableRowBase
    {
        public string title;
        public string description;
        public string successText;
        public string failureText;
        public Texture2D eventTexture;
        public EKingdomStatType collapsedStat;
        public EKingdomStatType resourceStat;
        public float successProbability;
        public int recoveryValue = 20;
        public int treasuryCost;
        public int publicSentimentCost;
        public int securityCost;

        public EmergencyRecoveryData ToRuntimeData()
        {
            return new EmergencyRecoveryData
            {
                recoveryId = rowID,
                title = title,
                description = description,
                successText = successText,
                failureText = failureText,
                eventImage = eventTexture != null ? eventTexture.ToSprite() : null,
                collapsedStat = collapsedStat,
                resourceStat = resourceStat,
                successProbability = successProbability,
                recoveryValue = recoveryValue,
                successCost = new StatModifier(treasuryCost,
                                               publicSentimentCost,
                                               securityCost)
            };
        }
    }
}
