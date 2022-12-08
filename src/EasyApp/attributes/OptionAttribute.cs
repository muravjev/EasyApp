namespace EasyApp
{
    public class OptionAttribute : MemberAttribute
    {
        private static readonly OutputAttribute defaultOutput = new OutputAttribute("Options");

        public override MemberType Type => MemberType.Option;

        public override OutputAttribute DefaultOutput => defaultOutput;

        public OptionAttribute(char shortKey, string longKey, string description, string? name = null, bool isRequired = true)
            : base(name, shortKey.ToString(), longKey, description, isRequired) { }

        public OptionAttribute(string longKey, string description, string? name = null, bool isRequired = true)
            : base(name, null, longKey, description, isRequired) { }
    }
}
