namespace EasyApp
{
    public class FlagAttribute : MemberAttribute
    {
        private static readonly OutputAttribute defaultOutput = new OutputAttribute("Options");

        public override MemberType Type => MemberType.Flag;

        public override OutputAttribute DefaultOutput => defaultOutput;

        public FlagAttribute(char shortKey, string longKey, string description, bool isBreaker = false)
            : base(null, shortKey == default ? null : shortKey.ToString(), longKey, description, false, isBreaker) { }

        public FlagAttribute(string longKey, string description, bool isBreaker = false)
            : this(default, longKey, description, isBreaker) { }
    }
}
