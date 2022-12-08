using System.Diagnostics;

namespace EasyApp.parser.components
{
    public interface IKeyParser<TOptions>
    {
        bool Parse(ParserState<TOptions> state, string arg);
    }

    public sealed class KeyParser<TOptions> : IKeyParser<TOptions>
    {
        private readonly IKeyArgParser ArgParser;

        private readonly IKeyMemberLocator MemberLocator;

        private readonly IValueConverter ValueConverter;

        public KeyParser(IKeyArgParser argParser, IKeyMemberLocator memberLocator, IValueConverter converter)
        {
            ArgParser = argParser;
            MemberLocator = memberLocator;
            ValueConverter = converter;
        }

        public bool Parse(ParserState<TOptions> state, string arg)
        {
            var keyArg = ArgParser.Parse(arg);
            if (keyArg == null)
            {
                return false;
            }

            var (key, name, value) = keyArg;

            var member = MemberLocator.GetMember(key, name);
            if (member == null)
            {
                throw new EasyAppException($"Unknown Key '{arg}'.");
            }

            if (member.Attribute.Type == MemberType.Flag)
            {
                Debug.Assert(string.IsNullOrEmpty(value));

                if (member.Attribute.IsHelp)
                {
                    state.IsHelp = true;
                }

                member.SetValue(state.Result, true);
            }
            else
            {
                Debug.Assert(member.Attribute.Type == MemberType.Option);

                if (string.IsNullOrEmpty(value))
                {
                    if (state.Args.Count > 0)
                    {
                        value = state.Args.Pop();
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        throw new EasyAppException($"Missing value for Option '{key}{name}'.");
                    }
                }

                member.SetValue(state.Result, ValueConverter.Convert(member, value));
            }

            return true;
        }
    }
}
