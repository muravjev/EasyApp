namespace EasyApp
{
    public abstract class OptionsBase
    {
        [Help]
        [Output("Help")]
        public bool Help = false;

        [Version]
        [Output("Help")]
        public bool Version = false;

        [LogLevel]
        [Output("Flags", true)]
        public LogLevel LogLevel = LogLevel.Normal;

        [Verbose]
        [Output("Flags")]
        public bool Verbose = false;

        [Quiet]
        [Output("Flags")]
        public bool Queit = false;
    }
}
