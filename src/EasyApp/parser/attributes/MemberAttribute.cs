namespace EasyApp
{
    public enum MemberType
    {
        Flag,
        Option,
        Parameter
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public abstract class MemberAttribute : Attribute
    {
        // Whether this Member (Flag) is Help flag and no further parsing is required.
        public readonly bool IsHelp;

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

        public abstract MemberType Type { get; }

        public abstract OutputAttribute DefaultOutput { get; }

        protected MemberAttribute(string? name, string? shortKey, string? longKey, string description, bool isRequired, bool isHelp = false)
        {
            Name = name;
            ShortKey = shortKey;
            LongKey = longKey;
            Description = description;
            IsRequired = isRequired;
            IsHelp = isHelp;
        }
    }
}
