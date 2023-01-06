using System.Text;

namespace EasyApp
{
    internal static class MemberHelper
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

            private void appendSectionUsageFlag(bool longKeysCanBeAligned)
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
                    if (longKeysCanBeAligned && Settings.UsageSectionAlignLongKey)
                    {
                        Sb.Append(' ', "-x".Length + Settings.UsageSectionKeysSeparator.Length);
                    }

                    appendLongKey();
                }
            }

            private void appendSectionUsageOption(bool longKeysCanBeAligned)
            {
                appendSectionUsageFlag(longKeysCanBeAligned);

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

            internal string ToSectionUsage(bool longKeysCanBeAligned)
            {
                Sb.Length = 0;

                switch (Attribute.Type)
                {
                    case MemberType.Flag:
                        appendSectionUsageFlag(longKeysCanBeAligned);
                        break;

                    case MemberType.Option:
                        appendSectionUsageOption(longKeysCanBeAligned);
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

        public static string ToInlineUsage(this Member member, EasyAppSettings settings)
        {
            return new Builder(member, settings).ToInlineUsage();
        }

        public static string ToSectionUsage(this Member member, EasyAppSettings settings, bool longKeysCanBeAligned)
        {
            return new Builder(member, settings).ToSectionUsage(longKeysCanBeAligned);
        }
    }
}
