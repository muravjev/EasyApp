namespace EasyApp
{
    public sealed class QuietAttribute : FlagAttribute
    {
        public QuietAttribute(char shortKey = default, string longKey = "quiet", string description = "Turn off logging.")
            : base(shortKey, longKey, description) { }
    }
}
