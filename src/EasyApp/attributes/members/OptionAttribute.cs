namespace EasyApp
{
    public class OptionAttribute : MemberAttribute
    {
        private static readonly SectionAttribute defaultSection = new OptionsSectionAttribute();

        public override MemberType Type => MemberType.Option;

        public override SectionAttribute DefaultSection => defaultSection;

        public OptionAttribute(char shortKey, string longKey, string description, string? name = null, bool isRequired = true)
            : base(name, shortKey.ToString(), longKey, description, isRequired) { }

        public OptionAttribute(string longKey, string description, string? name = null, bool isRequired = true)
            : base(name, null, longKey, description, isRequired) { }
    }
}
