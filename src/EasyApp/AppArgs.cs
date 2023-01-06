using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EasyApp
{
    public sealed class Result<T>
    {
        // Parse options.
        public readonly T Options;

        // Whether execution is breaked and help is required (no args or breakable flag like --version).
        public readonly bool IsBreaked = false;

        // Whether there were an exception during parsing.
        public readonly Exception? Exception = null;

        // Whether parsing is succeesfull.
        public bool IsParsed => Exception == null;

        public Result(T options, bool isBreaked)
        {
            Options = options;
            IsBreaked = isBreaked;
        }

        public Result(T options, Exception exception)
        {
            Options = options;
            Exception = exception;
        }
    }

    public interface IAppArgs
    {
        Result<T> Parse<T>(string[] args) where T : new();
    }

    internal sealed class Parser<T> where T : new()
    {
        private readonly T Result;

        private readonly Stack<string> Args;

        private bool ParseKeys = true;

        private bool IsBreaked = false;

        private int ParameterIndex = 0;

        private readonly Member[] Members;

        private readonly Dictionary<string, Member> ShortKeyMembers;

        private readonly Dictionary<string, Member> LongKeyMembers;

        private readonly Member[] ParameterMembers;

        internal Parser(string[] args, Member[] members, Dictionary<string, Member> shortKeyMembers, Dictionary<string, Member> longKeyMembers, Member[] parameters)
        {
            Result = new T();
            Args = new Stack<string>(args);
            IsBreaked = args.Length == 0;
            Members = members;
            ShortKeyMembers = shortKeyMembers;
            LongKeyMembers = longKeyMembers;
            ParameterMembers = parameters;
        }

        private Member? getKeyMember(string key, string name)
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

        private void setTypedValue(Member member, string value)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(member.Type);
                var converted = converter.ConvertFromInvariantString(value);

                member.SetValue(Result, converted);
            }
            catch (Exception e)
            {
                throw new AppException($"Failed to conert '{value}' as {member.Type.Name}.", e);
            }
        }

        private bool parseKey(string arg)
        {
            if (!ParseKeys)
            {
                return false;
            }

            if (arg == "--")
            {
                ParseKeys = false;
                return true;
            }

            var regex = new Regex(@"(--|-|\/)([^:=]+)[:=]?(.*)?", RegexOptions.Compiled);
            var matches = regex.Matches(arg);
            if (matches.Count == 0)
            {
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    throw new AppException($"Invalid Key '{arg}'.");
                }

                return false;
            }

            Debug.Assert(matches.Count == 1);

            var values = matches[0].Groups.Values.Skip(1).Select(x => x.Value).ToArray();
            Debug.Assert(values.Length == 3);

            var key = values[0];
            var name = values[1];
            var value = values[2];

            var member = getKeyMember(key, name);
            if (member == null)
            {
                throw new AppException($"Unknown Key '{arg}'.");
            }

            if (member.Attribute.Type == MemberType.Flag)
            {
                Debug.Assert(string.IsNullOrEmpty(value));

                if (member.Attribute.IsBreaker)
                {
                    IsBreaked = true;
                }

                member.SetValue(Result, true);
            }
            else
            {
                Debug.Assert(member.Attribute.Type == MemberType.Option);

                if (string.IsNullOrEmpty(value))
                {
                    if (Args.Count > 0)
                    {
                        value = Args.Pop();
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        throw new AppException($"Missing value for Option '{key}{name}'.");
                    }
                }

                setTypedValue(member, value);
            }

            return true;
        }

        private void parseParameter(string arg)
        {
            if (ParameterIndex >= ParameterMembers.Length)
            {
                throw new AppException($"Unexpected parameter '{arg}'");
            }

            var parameter = ParameterMembers[ParameterIndex++];

            setTypedValue(parameter, arg);
        }

        private void validateThatRequiredMemberssAreFilled()
        {
            foreach (var member in Members)
            {
                if (member.Attribute.IsRequired && member.GetValue(Result) == null)
                {
                    throw new AppException($"Value for {member.Attribute.Type.ToString().ToLower()} '{member.Attribute.Name}' is required.");
                }
            }
        }

        internal void parse()
        {
            while (Args.Count > 0 && IsBreaked == false)
            {
                var arg = Args.Pop();

                if (parseKey(arg))
                {
                    continue;
                }

                parseParameter(arg);
            }

            if (IsBreaked)
            {
                return;
            }

            validateThatRequiredMemberssAreFilled();
        }

        internal Result<T> Parse()
        {
            try
            {
                parse();
            }
            catch (AppException exception)
            {
                return new Result<T>(Result, exception);
            }
            catch (Exception exception)
            {
                return new Result<T>(Result, new AppException("Unknown parse exception.", exception));
            }

            return new Result<T>(Result, IsBreaked);
        }
    }

    public sealed class AppArgs : IAppArgs
    {
        public Result<T> Parse<T>(params string[] args) where T : new()
        {
            var members = AppReflector.CollectMembers<T>();

            var shortKeyMembers = members.GroupByKey(x => x.Attribute.ShortKey);
            var longKeyMembers = members.GroupByKey(x => x.Attribute.LongKey);
            var parameterMembers = members.Filter(MemberType.Parameter);

            var nonEmptyArgs = args.Where(x => !string.IsNullOrEmpty(x)).Reverse().ToArray();

            var parser = new Parser<T>(nonEmptyArgs, members, shortKeyMembers, longKeyMembers, parameterMembers);

            return parser.Parse();
        }
    }
}
