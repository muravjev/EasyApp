namespace EasyApp
{
    public class ParameterAttribute : FieldAttribute
    {
        private static readonly OutputAttribute defaultOutput = new OutputAttribute("Parameters");

        public override MemberType Type => MemberType.Parameter;

        public override OutputAttribute DefaultOutput => defaultOutput;

        public ParameterAttribute(string name, string description, bool isRequired = true)
            : base(name, null, null, description, isRequired) { }
    }
}
