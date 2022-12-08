using System.Text;

namespace EasyApp
{
    public sealed class EasyAppSettings
    {
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
