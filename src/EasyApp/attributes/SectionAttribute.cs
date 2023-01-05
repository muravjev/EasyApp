namespace EasyApp
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public abstract class SectionAttribute : Attribute
    {
        /// <summary>
        /// Order of the section in Usage line and Usage groups.
        /// </summary>
        public readonly int Order;

        /// <summary>
        /// Name of the section in Usage line and Usage groups.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Whether section could be expanded in Usage line. 
        /// </summary>
        public readonly bool Expandable;

        /// <summary>
        /// Use section as a separate Usage line.
        /// </summary>
        public readonly bool Standalone;

        protected SectionAttribute(int order, string name, bool expandable = false, bool standalone = false)
        {
            Order = order;
            Name = name;
            Expandable = expandable;
            Standalone = standalone;
        }
    }
}
