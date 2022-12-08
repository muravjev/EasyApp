using System.Diagnostics;

namespace EasyApp.parser.components
{
    public interface IKeyMemberLocator
    {
        Member? GetMember(string key, string name);
    }

    public sealed class KeyMemberLocator : IKeyMemberLocator
    {
        private readonly Dictionary<string, Member> ShortKeyMembers;

        private readonly Dictionary<string, Member> LongKeyMembers;

        public KeyMemberLocator(Dictionary<string, Member> shortKeyMembers, Dictionary<string, Member> longKeyMembers)
        {
            ShortKeyMembers = shortKeyMembers;
            LongKeyMembers = longKeyMembers;
        }

        public Member? GetMember(string key, string name)
        {
            if (key == "-")
            {
                return ShortKeyMembers.GetValueOrDefault(name);
            }

            if (key == "--")
            {
                return LongKeyMembers.GetValueOrDefault(name);
            }

            Debug.Assert(key == "/");

            return LongKeyMembers.GetValueOrDefault(name) ?? ShortKeyMembers.GetValueOrDefault(name);
        }
    }
}
