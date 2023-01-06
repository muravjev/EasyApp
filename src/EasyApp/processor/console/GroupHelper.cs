using System.Text;

namespace EasyApp
{
    internal sealed class Group
    {
        public readonly SectionAttribute Attribute;

        public readonly Member[] Members;

        public Group(SectionAttribute attribute, Member[] members)
        {
            Attribute = attribute;
            Members = members;
        }
    }

    internal record GroupMember(Member member, string key);

    internal sealed class GroupData
    {
        public readonly SectionAttribute Attribute;

        public readonly GroupMember[] Members;

        public readonly bool SectionIndentIsCrossed;

        public GroupData(SectionAttribute attribute, GroupMember[] members, bool sectionIndentIsCrossed)
        {
            Attribute = attribute;
            Members = members;
            SectionIndentIsCrossed = sectionIndentIsCrossed;
        }
    }

    internal sealed class GroupsData
    {
        public readonly GroupData[] Groups;

        public readonly int UsageSectionIndentLength;

        public GroupsData(GroupData[] groups, int sectionIndentLength)
        {
            Groups = groups;
            UsageSectionIndentLength = sectionIndentLength;
        }
    }

    internal static class GroupHelper
    {
        public sealed class Builder
        {
            private readonly Member Member;

            private readonly EasyAppSettings Settings;

            private readonly StringBuilder Sb;

            public MemberAttribute Attribute { get { return Member.Attribute; } }

            public Builder(Member member, EasyAppSettings settings)
            {
                Member = member;
                Settings = settings;
                Sb = new StringBuilder();
            }

            private void appendShortKey()
            {
                Sb.Append($"{Settings.ShortKeyPrefix}{Attribute.ShortKey}");
            }

            private void appendLongKey()
            {
                Sb.Append($"{Settings.LongKeyPrefix}{Attribute.LongKey}");
            }

            private void appendName()
            {
                Sb.Append(Attribute.Name ?? Member.Name.ToLower());
            }

            private void appendInlineUsageFlag()
            {
                var isShortKey = !string.IsNullOrEmpty(Attribute.ShortKey);
                var isLongKey = !string.IsNullOrEmpty(Attribute.LongKey);

                Sb.Append(" [");

                if (isShortKey && isLongKey)
                {
                    appendShortKey();
                    Sb.Append(Settings.UsageInlineKeysSeparator);
                    appendLongKey();
                }
                else if (isShortKey)
                {
                    appendShortKey();
                }
                else
                {
                    appendLongKey();
                }

                Sb.Append("]");
            }

            private void appendInlineUsageParameter()
            {
                Sb.Append(Attribute.IsRequired ? " <" : " [");
                appendName();
                Sb.Append(Attribute.IsRequired ? '>' : ']');
            }

            internal string ToInlineUsage()
            {
                Sb.Length = 0;

                switch (Attribute.Type)
                {
                    case MemberType.Flag:
                        appendInlineUsageFlag();
                        break;

                    case MemberType.Option:
                        throw new Exception($"Section {Attribute.Type} is not expandable");

                    case MemberType.Parameter:
                        appendInlineUsageParameter();
                        break;

                    case MemberType.Command:
                        throw new Exception($"Section {Attribute.Type} is not expandable");

                    default:
                        throw new Exception($"Unexpected MemeberType {Attribute.Type}");
                }

                return Sb.ToString();
            }

            private void appendSectionUsageFlag()
            {
                var isShortKey = !string.IsNullOrEmpty(Attribute.ShortKey);
                var isLongKey = !string.IsNullOrEmpty(Attribute.ShortKey);

                Sb.Append(Settings.UsageSectionKeyPrefix);

                if (isShortKey && isLongKey)
                {
                    appendShortKey();
                    Sb.Append(Settings.UsageSectionKeysSeparator);
                    appendLongKey();
                }
                else if (isShortKey)
                {
                    appendShortKey();
                }
                else
                {
                    // TODO: If we have only long keys, no alignment should be done? In all sections?

                    if (Settings.UsageSectionAlignLongKey)
                    {
                        Sb.Append(' ', "-?".Length + Settings.UsageSectionKeysSeparator.Length);
                    }

                    appendLongKey();
                }
            }

            private void appendSectionUsageOption()
            {
                appendSectionUsageFlag();

                Sb.Append(" <");
                appendName();
                Sb.Append(">");
            }

            private void appendSectionUsageParameter()
            {
                Sb.Append(Settings.UsageSectionKeyPrefix);
                Sb.Append(Attribute.IsRequired ? '<' : '[');
                appendName();
                Sb.Append(Attribute.IsRequired ? '>' : ']');
            }

            private void appendSectionUsageCommand()
            {
                Sb.Append(Settings.UsageSectionKeyPrefix);
                appendName();
            }

            internal string ToSectionUsage()
            {
                Sb.Length = 0;

                switch (Attribute.Type)
                {
                    case MemberType.Flag:
                        appendSectionUsageFlag();
                        break;

                    case MemberType.Option:
                        appendSectionUsageOption();
                        break;

                    case MemberType.Parameter:
                        appendSectionUsageParameter();
                        break;

                    case MemberType.Command:
                        appendSectionUsageCommand();
                        break;

                    default:
                        throw new Exception($"Unexpected MemeberType {Attribute.Type}");
                }

                return Sb.ToString();
            }
        }

        public static Group[] ToGroups(this Member[] members, bool showAll)
        {
            return members
                .GroupBy(x => x.Section)
                .OrderBy(x => x.Key.Order)
                .Select(x => new Group(x.Key, x.Where(m => showAll || !m.IsHidden).ToArray()))
                .ToArray();
        }

        public static GroupsData ToGroupsData(this Group[] groups, EasyAppSettings settings)
        {
            var sectionIndentLength = 0;
            var sectionMaxIndent = settings.UsageSectionMaxIndent - settings.UsageSectionKeyPosfix.Length;
            var groupsData = new List<GroupData>();

            foreach (var group in groups)
            {
                var longKeysCanBeAligned = group.Members.Any(x => !string.IsNullOrEmpty(x.Attribute.ShortKey));
                var members = group.Members.Select(m => new GroupMember(m, m.ToSectionUsage(settings, longKeysCanBeAligned))).ToArray();
                var sectionIndentMaxLength = members.Select(x => x.key.Length).Max();
                var sectionIndentIsCrossed = sectionIndentMaxLength > sectionMaxIndent;

                sectionIndentLength = Math.Max(sectionIndentLength, sectionIndentMaxLength);


                groupsData.Add(new GroupData(group.Attribute, members, sectionIndentIsCrossed));
            }

            sectionIndentLength = Math.Min(sectionIndentLength, sectionMaxIndent);

            return new GroupsData(groupsData.ToArray(), sectionIndentLength);
        }
    }
}
