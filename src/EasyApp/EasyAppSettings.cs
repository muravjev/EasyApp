using System.Text;

namespace EasyApp
{
    public sealed class EasyAppSettings
    {
        public int UnhandledExceptionExitCode = -1;
        public int ParseErrorExitCode = -2;

        public sealed class OutputSettings
        {
            public Encoding Encoding = Encoding.Unicode;
        }

        public OutputSettings Output = new OutputSettings
        {
            Encoding = Encoding.Unicode,
        };
    }
}
