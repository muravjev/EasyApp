namespace EasyApp
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    {
        // Usage section where Option/Parameter is put.
        public readonly string Section;

        // Whether Option/Parameter is hidden by default in Usage output.
        public readonly bool IsHidden;

        // Order of Option/Parameter in Usage output.
        public readonly int Order;

        public OutputAttribute(string section, bool isHidden = false)
        {
            Section = section;
            IsHidden = isHidden;
        }
    }
}
