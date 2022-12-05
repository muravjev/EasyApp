using System.Reflection;

namespace EasyApp
{
    public interface Member
    {
        FieldAttribute Attribute { get; }

        int MetadataToken { get; }

        string Name { get; }

        Type Type { get; }

        object? GetValue(object? instance);

        void SetValue(object? instance, object? value);
    }

    public abstract class MemberBase
    {
        protected readonly FieldAttribute Attribute;

        protected MemberBase(FieldAttribute attribute)
        {
            Attribute = attribute;
        }
    }

    public sealed class FieldMember : MemberBase, Member
    {
        private readonly FieldInfo FieldInfo;

        public FieldMember(FieldInfo fieldInfo, FieldAttribute attribute) : base(attribute)
        {
            FieldInfo = fieldInfo;
        }

        FieldAttribute Member.Attribute => Attribute;

        int Member.MetadataToken => FieldInfo.MetadataToken;

        string Member.Name => FieldInfo.Name;

        Type Member.Type => FieldInfo.FieldType;

        object? Member.GetValue(object? instance)
        {
            return FieldInfo.GetValue(instance);
        }

        void Member.SetValue(object? instance, object? value)
        {
            FieldInfo.SetValue(instance, value);
        }
    }

    public sealed class PropertyMember : MemberBase, Member
    {
        private readonly PropertyInfo PropertyInfo;

        public PropertyMember(PropertyInfo fieldInfo, FieldAttribute attribute) : base(attribute)
        {
            PropertyInfo = fieldInfo;
        }

        FieldAttribute Member.Attribute => Attribute;

        int Member.MetadataToken => PropertyInfo.MetadataToken;

        string Member.Name => PropertyInfo.Name;

        Type Member.Type => PropertyInfo.PropertyType;

        object? Member.GetValue(object? instance)
        {
            return PropertyInfo.GetValue(instance);
        }

        void Member.SetValue(object? instance, object? value)
        {
            PropertyInfo.SetValue(instance, value);
        }
    }
}
