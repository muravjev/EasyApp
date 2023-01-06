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

        [Verbosity]
        [FlagsSection]
        [IsHidden]
        public Verbosity Verbosity = Verbosity.Normal;

        [Minimal]
        [FlagsSection]
        public bool Minimal { set { Verbosity = Verbosity.Minimal; } }

        [Detailed]
        [FlagsSection]
        public bool Detailed { set { Verbosity = Verbosity.Detailed; } }

        [Quiet]
        [FlagsSection]
        public bool Queit { set { Verbosity = Verbosity.Quiet; } }
    }
}
