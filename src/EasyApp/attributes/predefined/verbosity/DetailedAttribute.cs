namespace EasyApp
{
    public sealed class DetailedAttribute : FlagAttribute
    {
        public DetailedAttribute(char shortKey = default, string longKey = "detailed", string description = "Turn on verbose logging.")
            : base(shortKey, longKey, description) { }
    }
}
