namespace EasyApp
{
    public class ParameterAttribute : FieldAttribute
    {
        public ParameterAttribute(int order, string name, string description, bool isRequired = true)
            : base(order, name, null, null, description, isRequired) { }

        public ParameterAttribute(string name, string description, bool isRequired = true)
            : this(1, name, description, isRequired) { }
    }
}
