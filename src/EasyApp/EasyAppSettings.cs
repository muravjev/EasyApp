using System.Text;

namespace EasyApp
{
    public abstract class EasyAppSettings
    {
        public int UnhandledExceptionExitCode = -1;
        public int ParseErrorExitCode = -2;

        public string KeyUsageInlineSeparator = " | ";
        public string KeyUsageSectionSeparator = ", ";

        public string ShortKeyPrefix = "-";
        public string LongKeyPrefix = "--";

        public Encoding OutputEncoding = Encoding.Unicode;
    }
}
