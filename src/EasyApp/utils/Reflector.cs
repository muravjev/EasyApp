using System.Reflection;

namespace EasyApp
{
    internal abstract class Member
    {
        public readonly MemberAttribute Attribute;

        public readonly SectionAttribute Section;

        public readonly bool IsHidden;

        public readonly int MetadataToken;

        public readonly string Name;

        public readonly Type Type;

        public abstract object? GetValue(object? instance);

        public abstract void SetValue(object? instance, object? value);

        protected Member(MemberAttribute attribute, SectionAttribute? section, IsHiddenAttribute? isHidden, MemberInfo info, Type type)
        {
            Attribute = attribute;
            Section = section ?? attribute.DefaultSection;
            IsHidden = isHidden == null;
            MetadataToken = info.MetadataToken;
            Name = info.Name;
            Type = type;
        }
    }

    internal sealed class FieldMember : Member
    {
        private readonly FieldInfo FieldInfo;

        public FieldMember(MemberAttribute attribute, SectionAttribute? section, IsHiddenAttribute? isHidden, FieldInfo fieldInfo)
            : base(attribute, section, isHidden, fieldInfo, fieldInfo.FieldType)
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

    internal sealed class PropertyMember : Member
    {
        private readonly PropertyInfo PropertyInfo;

        public PropertyMember(MemberAttribute attribute, SectionAttribute? section, IsHiddenAttribute? isHidden, PropertyInfo propertyInfo)
            : base(attribute, section, isHidden, propertyInfo, propertyInfo.PropertyType)
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

    internal static class Reflector
    {
        public static Member[] CollectMembers<TOptions, TAttribute>() where TAttribute : MemberAttribute
        {
            var members = new List<Member>();

            foreach (var field in typeof(TOptions).GetFields())
            {
                var attr = field.GetCustomAttribute<TAttribute>();
                if (attr != null)
                {
                    var section = field.GetCustomAttribute<SectionAttribute>();
                    var isHidden = field.GetCustomAttribute<IsHiddenAttribute>();
                    members.Add(new FieldMember(attr, section, isHidden, field));
                }
            }

            foreach (var property in typeof(TOptions).GetProperties())
            {
                var attr = property.GetCustomAttribute<TAttribute>();
                if (attr != null)
                {
                    var section = property.GetCustomAttribute<SectionAttribute>();
                    var isHidden = property.GetCustomAttribute<IsHiddenAttribute>();
                    members.Add(new PropertyMember(attr, section, isHidden, property));
                }
            }

            return members.ToArray();
        }

        public static Member[] CollectMembers<TOptions>()
        {
            return CollectMembers<TOptions, MemberAttribute>();
        }

        public static Member[] Filter(this Member[] members, MemberType type)
        {
            return members.Where(x => x.Attribute.Type == type).ToArray();
        }

        public static Dictionary<string, Member> GroupByKey(this Member[] members, Func<Member, string?> selector)
        {
            var byKey = new Dictionary<string, Member>();

            foreach (var member in members)
            {
                var key = selector(member);

                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                byKey.Add(key, member);
            }

            return byKey;
        }

        public static TValue GetValue<TAttribute, TValue>(this Member[] members, object? options, TValue defaultValue)
        {
            var member = members.FirstOrDefault(x => x.Attribute.GetType() == typeof(TAttribute));
            if (member == null)
            {
                return defaultValue;
            }

            var value = member.GetValue(options);
            if (value == null)
            {
                return defaultValue;
            }

            return (TValue)value;
        }

        public static bool GetValue<TAttribute>(this Member[] members, object? options)
        {
            return members.GetValue<TAttribute, bool>(options, false);
        }
    }
}
