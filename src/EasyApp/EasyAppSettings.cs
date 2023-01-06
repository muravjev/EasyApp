using System.Text;

namespace EasyApp
{
    public enum MultiLine
    {
        Always,
        Never,
        Auto
    }

    public abstract class EasyAppSettings
    {
        public int UnhandledExceptionExitCode = -1;
        public int ParseErrorExitCode = -2;

        public string UsageInlineKeysSeparator = " | ";

        public int UsageSectionMaxIndent = 28;
        public string UsageSectionKeyPrefix = "  ";
        public string UsageSectionKeyPosfix = "  ";
        public string UsageSectionKeysSeparator = ", ";
        public MultiLine UsageSectionAddLineForMultiLine = MultiLine.Auto;
        public bool UsageSectionAlignLongKey = true;

        public string ShortKeyPrefix = "-";
        public string LongKeyPrefix = "--";

        public Encoding OutputEncoding = Encoding.Unicode;
    }
}
