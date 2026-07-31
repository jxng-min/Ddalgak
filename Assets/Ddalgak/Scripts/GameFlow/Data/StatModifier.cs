using System;

namespace Ddalgak
{
    [Serializable]
    public struct StatModifier
    {
        public int treasury;
        public int publicSentiment;
        public int security;

        public StatModifier(int treasury, int publicSentiment, int security)
        {
            this.treasury = treasury;
            this.publicSentiment = publicSentiment;
            this.security = security;
        }

        public static StatModifier Zero => new(0, 0, 0);

        public static StatModifier operator +(StatModifier left, StatModifier right)
        {
            return new StatModifier(left.treasury + right.treasury,
                                    left.publicSentiment + right.publicSentiment,
                                    left.security + right.security);
        }
    }
}
