namespace EasyApp
{
    public sealed class VerboseAttribute : FlagAttribute
    {
        public VerboseAttribute(char shortKey = default, string longKey = "verbose", string description = "Turn on verbose logging.")
            : base(shortKey, longKey, description) { }
    }
}
