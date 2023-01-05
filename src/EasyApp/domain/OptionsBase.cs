namespace EasyApp
{
    public abstract class OptionsBase
    {
        [Version]
        [HelpSection]
        public bool Version = false;

        [Help]
        [HelpSection]
        public bool Help = false;

        [All]
        [HelpSection]
        public bool All = false;

        [LogLevel]
        [FlagsSection]
        [IsHidden]
        public LogLevel LogLevel = LogLevel.Normal;

        [Minimal]
        [FlagsSection]
        public bool Minimal { set { LogLevel = LogLevel.Minimal; } }

        [Verbose]
        [FlagsSection]
        public bool Verbose { set { LogLevel = LogLevel.Verbose; } }

        [Quiet]
        [FlagsSection]
        public bool Queit { set { LogLevel = LogLevel.Quiet; } }
    }
}
