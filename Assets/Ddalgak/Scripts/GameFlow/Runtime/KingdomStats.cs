using System;
using UnityEngine;

namespace Ddalgak
{
    [Serializable]
    public sealed class KingdomStats
    {
        public const int MinValue = 0;
        public const int MaxValue = 100;
        public const int DefaultValue = 50;

        [SerializeField] private int treasury = DefaultValue;
        [SerializeField] private int publicSentiment = DefaultValue;
        [SerializeField] private int security = DefaultValue;

        public int Treasury => treasury;
        public int PublicSentiment => publicSentiment;
        public int Security => security;

        public void Initialize(int initialTreasury = DefaultValue,
                               int initialPublicSentiment = DefaultValue,
                               int initialSecurity = DefaultValue)
        {
            treasury = Clamp(initialTreasury);
            publicSentiment = Clamp(initialPublicSentiment);
            security = Clamp(initialSecurity);
        }

        public void Apply(StatModifier modifier)
        {
            treasury = Clamp(treasury + modifier.treasury);
            publicSentiment = Clamp(publicSentiment + modifier.publicSentiment);
            security = Clamp(security + modifier.security);
        }

        public KingdomStatsSnapshot CreateSnapshot()
        {
            return new KingdomStatsSnapshot(treasury, publicSentiment, security);
        }

        private static int Clamp(int value)
        {
            return Mathf.Clamp(value, MinValue, MaxValue);
        }
    }
}
