using System;

namespace Ddalgak
{
    [Serializable]
    public sealed class EmergencyRecoveryData
    {
        public string recoveryId;
        public string title;
        public string description;
        public string successText;
        public string failureText;
        public UnityEngine.Sprite eventImage;
        public EKingdomStatType collapsedStat;
        public EKingdomStatType resourceStat;
        public float successProbability;
        public int recoveryValue = 20;
        public StatModifier successCost;
    }
}
