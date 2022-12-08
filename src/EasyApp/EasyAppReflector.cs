using System.Reflection;

namespace EasyApp
{
    internal static class EasyAppReflector
    {
        internal static Member[] CollectMembers<TOptions>()
        {
            var members = new List<Member>();

            foreach (var field in typeof(TOptions).GetFields())
            {
                var attr = field.GetCustomAttribute<MemberAttribute>();
                if (attr != null)
                {
                    var output = field.GetCustomAttribute<OutputAttribute>();
                    members.Add(new FieldMember(attr, output, field));
                }
            }

            foreach (var property in typeof(TOptions).GetProperties())
            {
                var attr = property.GetCustomAttribute<MemberAttribute>();
                if (attr != null)
                {
                    var output = property.GetCustomAttribute<OutputAttribute>();
                    members.Add(new PropertyMember(attr, output, property));
                }
            }

            return members.ToArray();
        }

        internal static Member[] Filter(this Member[] members, MemberType type)
        {
            return members.Where(x => x.Attribute.Type == type).ToArray();
        }

        internal static Dictionary<string, Member> GroupByKey(this Member[] members, Func<Member, string?> selector)
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

        internal static TValue GetValue<TAttribute, TOptions, TValue>(this Member[] members, TOptions options, TValue defaultValue)
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

        internal static bool GetValue<TAttribute, TOptions>(this Member[] members, TOptions options)
        {
            return members.GetValue<TAttribute, TOptions, bool>(options, false);
        }
    }
}
