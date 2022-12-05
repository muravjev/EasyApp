namespace EasyApp
{
    public class FlagAttribute : FieldAttribute
    {
        public FlagAttribute(int order, char shortKey, string longKey, string description, bool isBreaker = false)
            : base(order, null, shortKey == default ? null : shortKey.ToString(), longKey, description, false, isBreaker) { }

        public FlagAttribute(char shortKey, string longKey, string description, bool isBreaker = false)
            : this(1, shortKey, longKey, description, isBreaker) { }

        public FlagAttribute(string longKey, string description, bool isBreaker = false)
            : this(1, default, longKey, description, isBreaker) { }
    }
}
