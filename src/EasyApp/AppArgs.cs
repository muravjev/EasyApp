using System.ComponentModel;
using System.Reflection;

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

        private readonly Dictionary<string, Member> Flags;

        private readonly Dictionary<string, Member> Options;

        private readonly Member[] Parameters;

        internal Parser(string[] args, Dictionary<string, Member> flags, Dictionary<string, Member> options, Member[] parameters)
        {
            Result = new T();
            Args = new Stack<string>(args);
            IsBreaked = args.Length == 0;
            Flags = flags;
            Options = options;
            Parameters = parameters;
        }

        private void setFieldValue(Member member, string value)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(member.Type);
                var converted = converter.ConvertFromInvariantString(value);

                member.SetValue(Result, converted);
            }
            catch (Exception e)
            {
                throw new AppException($"Failed to parse '{value}' as {member.Type.Name}.", e);
            }
        }

        private bool parseSkipper(string arg)
        {
            if (arg == "--")
            {
                ParseKeys = false;
            }

            return !ParseKeys;
        }

        private bool parseFlag(string arg)
        {
            var flag = Flags.GetValueOrDefault(arg);
            if (flag == null)
            {
                return false;
            }

            flag.SetValue(Result, true);

            if (flag.Attribute.IsBreaker)
            {
                IsBreaked = true;
            }

            return true;
        }

        private string? popNextNotNullValue()
        {
            while (Args.Count > 0)
            {
                var value = Args.Pop();

                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

        private bool parseOption(string arg)
        {
            var option = Options.GetValueOrDefault(arg);
            if (option == null)
            {
                return false;
            }

            var value = popNextNotNullValue();

            if (value == null)
            {
                throw new AppException($"Missing option '{option.Attribute.Name}' value '{arg}'");
            }

            setFieldValue(option, value);
            return true;
        }

        private bool parseOption(string arg, char separator)
        {
            var i = arg.IndexOf(separator);
            if (i == -1)
            {
                return false;
            }

            var option = Options.GetValueOrDefault(arg.Substring(0, i));
            if (option == null)
            {
                return false;
            }

            setFieldValue(option, arg.Substring(i + 1));
            return true;
        }

        private void validateThatKeysAreParsed(string arg)
        {
            if (arg.StartsWith("-") || arg.StartsWith("/"))
            {
                throw new AppException($"Unknown key '{arg}'.");
            }
        }

        private void parseParameter(string arg)
        {
            if (ParameterIndex >= Parameters.Length)
            {
                throw new AppException($"Unexpected parameter '{arg}'");
            }

            var parameter = Parameters[ParameterIndex++];

            setFieldValue(parameter, arg);
        }

        private void validateThatRequiredFieldsAreFilled(IEnumerable<Member> members, string name)
        {
            foreach (var member in members)
            {
                if (member.Attribute.IsRequired && member.GetValue(Result) == null)
                {
                    throw new AppException($"Value for {name} '{member.Attribute.Name}' is required.");
                }
            }
        }

        internal void parse()
        {
            while (Args.Count > 0 && IsBreaked == false)
            {
                var arg = Args.Pop();

                if (string.IsNullOrEmpty(arg))
                {
                    continue;
                }

                if (ParseKeys)
                {
                    if (parseSkipper(arg))
                    {
                        continue;
                    }

                    if (parseFlag(arg))
                    {
                        continue;
                    }

                    if (parseOption(arg))
                    {
                        continue;
                    }

                    if (parseOption(arg, ':'))
                    {
                        continue;
                    }

                    if (parseOption(arg, '='))
                    {
                        continue;
                    }

                    validateThatKeysAreParsed(arg);
                }

                parseParameter(arg);
            }

            if (IsBreaked)
            {
                return;
            }

            validateThatRequiredFieldsAreFilled(Options.Values, "option");
            validateThatRequiredFieldsAreFilled(Parameters, "parameter");
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
        internal static Member[] CollectMembers<TOptions, TAttribute>() where TAttribute : FieldAttribute
        {
            var fields = typeof(TOptions)
                .GetFields()
                .Where(field => Attribute.IsDefined(field, typeof(TAttribute)))
                .Select(field => (Member)new FieldMember(field, field.GetCustomAttribute<TAttribute>()!));

            var props = typeof(TOptions)
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(TAttribute)))
                .Select(prop => (Member)new PropertyMember(prop, prop.GetCustomAttribute<TAttribute>()!));

            var members = fields.Concat(props)
                .OrderBy(member => member.Attribute.Order)
                .ThenBy(member => member.MetadataToken)
                .ToArray();

            return members;
        }

        private static Dictionary<string, Member> toFieldsByKeyMap(Member[] members)
        {
            var byKey = new Dictionary<string, Member>();

            foreach (var member in members)
            {
                var attr = member.Attribute;

                if (!string.IsNullOrEmpty(attr.ShortKey))
                {
                    byKey.Add($"-{attr.ShortKey}", member);
                    byKey.Add($"/{attr.ShortKey}", member);
                }

                if (!string.IsNullOrEmpty(attr.LongKey))
                {
                    byKey.Add($"--{attr.LongKey}", member);
                    byKey.Add($"/{attr.LongKey}", member);
                }
            }

            return byKey;
        }

        public Result<T> Parse<T>(params string[] args) where T : new()
        {
            var flagsFields = CollectMembers<T, FlagAttribute>();
            var optionsFields = CollectMembers<T, OptionAttribute>();
            var parametersFields = CollectMembers<T, ParameterAttribute>();

            var flagsByKey = toFieldsByKeyMap(flagsFields);
            var optionsByKey = toFieldsByKeyMap(optionsFields);

            var nonEmptyArgs = args.Where(x => !string.IsNullOrEmpty(x)).Reverse().ToArray();

            var parser = new Parser<T>(nonEmptyArgs, flagsByKey, optionsByKey, parametersFields);

            return parser.Parse();
        }
    }
}
