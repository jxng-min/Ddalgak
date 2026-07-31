namespace Ddalgak
{
    public readonly struct KingdomStatsSnapshot
    {
        public int Treasury { get; }
        public int PublicSentiment { get; }
        public int Security { get; }

        public KingdomStatsSnapshot(int treasury, int publicSentiment, int security)
        {
            Treasury = treasury;
            PublicSentiment = publicSentiment;
            Security = security;
        }
    }
}
