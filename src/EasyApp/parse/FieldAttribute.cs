namespace EasyApp
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public abstract class FieldAttribute : Attribute
    {
        // Parameters order parsing! Order of Flags, Options, Parameters in Usage.
        public readonly int Order;

        // Whether this Member (Flag) will break the further parsing.
        public readonly bool IsBreaker;

        // Parameter or Option <name>. Used in Usage and errors.
        public readonly string? Name;

        // Short key used in Flag and Option parsing and usage as -<key> /<key>.
        public readonly string? ShortKey;

        // Long key used in Flag and Option parsing and usage as --<key> /<key>.
        public readonly string? LongKey;

        // Description used in usage as description of Flag, Option or Parameter.
        public readonly string Description;

        // Used in validation of an Option and Parameter that value is not default.
        public readonly bool IsRequired;

        protected FieldAttribute(int order, string? name, string? shortKey, string? longKey, string description, bool isRequired, bool isBreaker = false)
        {
            Order = order;
            Name = name;
            ShortKey = shortKey;
            LongKey = longKey;
            Description = description;
            IsRequired = isRequired;
            IsBreaker = isBreaker;
        }
    }
}
