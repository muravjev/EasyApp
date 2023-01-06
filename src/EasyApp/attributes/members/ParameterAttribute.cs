namespace EasyApp
{
    public class ParameterAttribute : MemberAttribute
    {
        private static readonly SectionAttribute defaultSection = new ParametersSectionAttribute();

        public override MemberType Type => MemberType.Parameter;

        public override SectionAttribute DefaultSection => defaultSection;

        public ParameterAttribute(string name, string description, bool isRequired = true)
            : base(name, null, null, description, isRequired) { }
    }
}
