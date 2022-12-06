namespace EasyApp
{
    public class OptionAttribute : FieldAttribute
    {
        public override MemberType Type => MemberType.Option;

        public OptionAttribute(int order, char shortKey, string longKey, string description, string? name = null, bool isRequired = true)
            : base(order, name, shortKey.ToString(), longKey, description, isRequired) { }

        public OptionAttribute(char shortKey, string longKey, string description, string? name = null, bool isRequired = true)
            : this(1, shortKey, longKey, description, name, isRequired) { }

        public OptionAttribute(int order, string longKey, string description, string? name = null, bool isRequired = true)
            : base(order, name, null, longKey, description, isRequired) { }

        public OptionAttribute(string longKey, string description, string? name = null, bool isRequired = true)
            : this(1, longKey, description, name, isRequired) { }
    }
}
