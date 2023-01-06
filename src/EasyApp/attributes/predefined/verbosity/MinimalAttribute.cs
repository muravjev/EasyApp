namespace EasyApp
{
    public sealed class MinimalAttribute : FlagAttribute
    {
        public MinimalAttribute(char shortKey = default, string longKey = "minimal", string description = "Turn on minimal logging.")
            : base(shortKey, longKey, description) { }
    }
}
