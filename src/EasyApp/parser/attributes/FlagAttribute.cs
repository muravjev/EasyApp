namespace EasyApp
{
    public class FlagAttribute : MemberAttribute
    {
        private static readonly OutputAttribute defaultOutput = new OutputAttribute("Options");

        public override MemberType Type => MemberType.Flag;

        public override OutputAttribute DefaultOutput => defaultOutput;

        public FlagAttribute(char shortKey, string longKey, string description, bool isHelp = false)
            : base(null, shortKey == default ? null : shortKey.ToString(), longKey, description, false, isHelp) { }

        public FlagAttribute(string longKey, string description, bool isHelp = false)
            : this(default, longKey, description, isHelp) { }
    }
}
