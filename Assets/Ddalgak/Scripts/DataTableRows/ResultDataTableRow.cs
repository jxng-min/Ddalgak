using JxModule.DataTable;

namespace Ddalgak
{
    public sealed class ResultDataTableRow : DataTableRowBase
    {
        public int treasury;
        public int publicSentiment;
        public int security;
        public string resultText;

        public StatModifier ToModifier()
        {
            return new StatModifier(treasury, publicSentiment, security);
        }
    }
}
