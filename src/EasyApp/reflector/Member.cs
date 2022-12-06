using System.Reflection;

namespace EasyApp
{
    public abstract class Member
    {
        public readonly MemberAttribute Attribute;

        public readonly OutputAttribute Output;

        public readonly int MetadataToken;

        public readonly string Name;

        public readonly Type Type;

        public abstract object? GetValue(object? instance);

        public abstract void SetValue(object? instance, object? value);

        protected Member(MemberAttribute attribute, OutputAttribute? output, MemberInfo info, Type type)
        {
            Attribute = attribute;
            Output = output ?? attribute.DefaultOutput;
            MetadataToken = info.MetadataToken;
            Name = info.Name;
            Type = type;
        }
    }

    public sealed class FieldMember : Member
    {
        private readonly FieldInfo FieldInfo;

        public FieldMember(MemberAttribute attribute, OutputAttribute? output, FieldInfo fieldInfo)
            : base(attribute, output, fieldInfo, fieldInfo.FieldType)
        {
            FieldInfo = fieldInfo;
        }

        public override object? GetValue(object? instance)
        {
            return FieldInfo.GetValue(instance);
        }

        public override void SetValue(object? instance, object? value)
        {
            FieldInfo.SetValue(instance, value);
        }
    }
    public sealed class PropertyMember : Member
    {
        private readonly PropertyInfo PropertyInfo;

        public PropertyMember(MemberAttribute attribute, OutputAttribute? output, PropertyInfo propertyInfo)
            : base(attribute, output, propertyInfo, propertyInfo.PropertyType)
        {
            PropertyInfo = propertyInfo;
        }

        public override object? GetValue(object? instance)
        {
            return PropertyInfo.GetValue(instance);
        }

        public override void SetValue(object? instance, object? value)
        {
            PropertyInfo.SetValue(instance, value);
        }
    }
}
