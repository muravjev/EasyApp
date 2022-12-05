namespace EasyApp
{
    public class FlagAttribute : FieldAttribute
    {
        public FlagAttribute(int order, char shortKey, string longKey, string description)
            : base(order, null, shortKey == default ? null : shortKey.ToString(), longKey, description, false) { }

        public FlagAttribute(char shortKey, string longKey, string description)
            : this(1, shortKey, longKey, description) { }

        public FlagAttribute(string longKey, string description)
            : base(1, longKey, null, longKey, description, false) { }
    }
}
