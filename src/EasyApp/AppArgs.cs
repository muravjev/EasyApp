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

    internal record Field<TAttribute>(IMember Member, TAttribute Attribute);

    public interface IMember
    {
        int MetadataToken { get; }

        string Name { get; }

        Type Type { get; }

        object? GetValue(object? instance);

        void SetValue(object? instance, object? value);
    }

    public sealed class FieldMember : IMember
    {
        private readonly FieldInfo FieldInfo;

        public FieldMember(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo;
        }

        int IMember.MetadataToken => FieldInfo.MetadataToken;

        string IMember.Name => FieldInfo.Name;

        Type IMember.Type => FieldInfo.FieldType;

        object? IMember.GetValue(object? instance)
        {
            return FieldInfo.GetValue(instance);
        }

        void IMember.SetValue(object? instance, object? value)
        {
            FieldInfo.SetValue(instance, value);
        }
    }

    public sealed class PropertyMember : IMember
    {
        private readonly PropertyInfo PropertyInfo;

        public PropertyMember(PropertyInfo fieldInfo)
        {
            PropertyInfo = fieldInfo;
        }

        int IMember.MetadataToken => PropertyInfo.MetadataToken;

        string IMember.Name => PropertyInfo.Name;

        Type IMember.Type => PropertyInfo.PropertyType;

        object? IMember.GetValue(object? instance)
        {
            return PropertyInfo.GetValue(instance);
        }

        void IMember.SetValue(object? instance, object? value)
        {
            PropertyInfo.SetValue(instance, value);
        }
    }

    internal sealed class Parser<T> where T : new()
    {
        private readonly T Result;

        private readonly Stack<string> Args;

        private bool ParseKeys = true;

        private bool IsBreaked = false;

        private int ParameterIndex = 0;

        private readonly Dictionary<string, Field<FlagAttribute>> Flags;

        private readonly Dictionary<string, Field<OptionAttribute>> Options;

        private readonly Field<ParameterAttribute>[] Parameters;

        internal Parser(string[] args, Dictionary<string, Field<FlagAttribute>> flags, Dictionary<string, Field<OptionAttribute>> options, Field<ParameterAttribute>[] parameters)
        {
            Result = new T();
            Args = new Stack<string>(args);
            IsBreaked = args.Length == 0;
            Flags = flags;
            Options = options;
            Parameters = parameters;
        }

        private void setFieldValue(IMember member, string value)
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

            flag.Member.SetValue(Result, true);

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

            setFieldValue(option.Member, value);
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

            setFieldValue(option.Member, arg.Substring(i + 1));
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

            setFieldValue(parameter.Member, arg);
        }

        private void validateThatRequiredFieldsAreFilled<TAttribute>(IEnumerable<Field<TAttribute>> fields, string name) where TAttribute : FieldAttribute
        {
            foreach (var field in fields)
            {
                if (field.Attribute.IsRequired && field.Member.GetValue(Result) == null)
                {
                    throw new AppException($"Value for {name} '{field.Attribute.Name}' is required.");
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
        internal static Field<TAttribute>[] CollectMembers<TOptions, TAttribute>() where TAttribute : FieldAttribute
        {
            var fields = typeof(TOptions)
                .GetFields()
                .Where(field => Attribute.IsDefined(field, typeof(TAttribute)))
                .Select(field => new Field<TAttribute>(new FieldMember(field), field.GetCustomAttribute<TAttribute>()!));

            var props = typeof(TOptions)
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(TAttribute)))
                .Select(prop => new Field<TAttribute>(new PropertyMember(prop), prop.GetCustomAttribute<TAttribute>()!));

            var members = fields.Concat(props)
                .OrderBy(member => member.Attribute.Order)
                .ThenBy(member => member.Member.MetadataToken)
                .ToArray();

            return members;
        }

        private static Dictionary<string, Field<T>> toFieldsByKeyMap<T>(Field<T>[] fields) where T : FieldAttribute
        {
            var byKey = new Dictionary<string, Field<T>>();

            foreach (var field in fields)
            {
                var attr = field.Attribute;

                if (!string.IsNullOrEmpty(attr.ShortKey))
                {
                    byKey.Add($"-{attr.ShortKey}", field);
                    byKey.Add($"/{attr.ShortKey}", field);
                }

                if (!string.IsNullOrEmpty(attr.LongKey))
                {
                    byKey.Add($"--{attr.LongKey}", field);
                    byKey.Add($"/{attr.LongKey}", field);
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
