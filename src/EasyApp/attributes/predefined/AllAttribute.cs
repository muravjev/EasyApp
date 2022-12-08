namespace EasyApp
{
    public sealed class AllAttribute : FlagAttribute
    {
        public AllAttribute(char shortKey = default, string longKey = "all", string description = "Display all options.")
            : base(shortKey, longKey, description, true) { }
    }
}
