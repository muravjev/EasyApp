namespace EasyApp
{
    public sealed class VersionAttribute : FlagAttribute
    {
        public VersionAttribute(char shortKey = 'v', string longKey = "version", string description = "Display version information.")
            : base(shortKey, longKey, description, true) { }
    }
}
