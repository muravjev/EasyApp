namespace EasyApp
{
    public sealed class HelpAttribute : FlagAttribute
    {
        public HelpAttribute(char shortKey = 'h', string longKey = "help", string description = "Display help.")
            : base(shortKey, longKey, description, true) { }
    }
}
