using System.Reflection;

namespace EasyApp
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class KeyAttribute : Attribute
    {
        public readonly int Order;

        public readonly string Name;

        public readonly string? ShortKey;

        public readonly string? LongKey;

        public readonly string Description;

        public readonly bool IsRequired;

        protected KeyAttribute(int order, string name, string? shortKey, string? longKey, string description, bool isRequired)
        {
            Order = order;
            Name = name;
            ShortKey = shortKey;
            LongKey = longKey;
            Description = description;
            IsRequired = isRequired;
        }

        protected KeyAttribute(int order, string name, string description, bool isRequired)
            : this(order, name, null, null, description, isRequired) { }

        protected KeyAttribute(int order, string shortKey, string longKey, string description, bool isRequired)
            : this(order, longKey ?? shortKey, shortKey, longKey, description, isRequired) { }

        protected KeyAttribute(int order, char shortKey, string longKey, string description, bool isRequired)
            : this(order, shortKey.ToString(), longKey, description, isRequired) { }
    }

    public class FlagAttribute : KeyAttribute
    {
        public bool IsBreaker = true;

        public FlagAttribute(int order, char shortKey, string longKey, string description)
            : base(order, shortKey, longKey, description, false) { }

        public FlagAttribute(char shortKey, string longKey, string description)
            : base(1, shortKey, longKey, description, false) { }
    }

    public class OptionAttribute : KeyAttribute
    {
        public string? ValueName = null;

        public OptionAttribute(int order, char shortKey, string longKey, string description, string? valueName = null, bool isRequired = true)
            : base(order, shortKey, longKey, description, isRequired)
        {
            ValueName = valueName;
        }

        public OptionAttribute(char shortKey, string longKey, string description, string? valueName = null, bool isRequired = true)
            : this(1, shortKey, longKey, description, valueName, isRequired) { }
    }

    public class ValueAttribute : KeyAttribute
    {
        public ValueAttribute(int order, string name, string description, bool isRequired = true)
            : base(order, name, description, isRequired) { }

        public ValueAttribute(string name, string description, bool isRequired = true)
            : this(1, name, description, isRequired) { }
    }

    public sealed class Result<T>
    {
        public readonly T Options;

        public readonly Stack<string> Errors;

        public readonly bool IsBreaked;

        public bool IsParsed => Errors.Count == 0;

        public bool HasErrors => Errors.Count > 0;

        public Result(T options, Stack<string> errors, bool isBreaked)
        {
            Options = options;
            Errors = errors;
            IsBreaked = isBreaked;
        }
    }

    public interface IAppArgs
    {
        Result<T> Parse<T>(string[] args) where T : new();
    }

    internal record Field<TAttribute>(FieldInfo FieldInfo, TAttribute Attribute);

    internal sealed class Parser<T> where T : new()
    {
        private readonly T Result;

        private readonly Stack<string> Args;

        private readonly Stack<string> Errors = new Stack<string>();

        private bool SkipKeys = false;

        private bool IsBreaked = false;

        private int ValueIndex = 0;

        private readonly Dictionary<string, Field<FlagAttribute>> Flags;

        private readonly Dictionary<string, Field<OptionAttribute>> Options;

        private readonly Field<ValueAttribute>[] Values;

        internal Parser(string[] args, Dictionary<string, Field<FlagAttribute>> flags, Dictionary<string, Field<OptionAttribute>> options, Field<ValueAttribute>[] values)
        {
            Result = new T();
            Args = new Stack<string>(args.Reverse());
            Flags = flags;
            Options = options;
            Values = values;
        }

        private bool setFieldValue(FieldInfo field, string value)
        {
            if (field.FieldType == typeof(string))
            {
                field.SetValue(Result, value);
                return true;
            }
            else if (field.FieldType == typeof(int))
            {
                if (int.TryParse(value, out int intValue))
                {
                    field.SetValue(Result, intValue);
                    return true;
                }
                else
                {
                    Errors.Push($"Failed to parse '{value}' as int.");
                }
            }
            else if (field.FieldType.IsEnum)
            {
                var enumValue = Enum.Parse(field.FieldType, value, true);
                field.SetValue(Result, enumValue);
                return true;
            }
            else
            {
                Errors.Push($"Unsupported type '{field.FieldType.Name}'.");
            }

            return false;
        }

        private bool parseSkipper(string arg)
        {
            if (arg == "--")
            {
                SkipKeys = true;
            }

            return SkipKeys;
        }

        private bool parseFlag(string arg)
        {
            var flag = Flags.GetValueOrDefault(arg);
            if (flag == null)
            {
                return false;
            }

            flag.FieldInfo.SetValue(Result, true);

            if (flag.Attribute.IsBreaker)
            {
                IsBreaked = true;
            }

            return true;
        }

        private bool parseOption(string arg)
        {
            var option = Options.GetValueOrDefault(arg);
            if (option == null)
            {
                return false;
            }

            if (Args.Count == 0)
            {
                Errors.Push($"Missing option '{option.Attribute.Name}' value '{arg}'");
            }
            else
            {
                setFieldValue(option.FieldInfo, Args.Pop());
            }

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

            setFieldValue(option.FieldInfo, arg.Substring(i + 1));
            return true;
        }

        private bool validateThatKeysAreParsed(string arg)
        {
            if (arg.StartsWith("-") || arg.StartsWith("/"))
            {
                Errors.Push($"Unknown key '{arg}'.");
                return true;
            }

            return false;
        }

        private void parseValue(string arg)
        {
            if (ValueIndex >= Values.Length)
            {
                Errors.Push($"Unexpected parameter '{arg}'");
                return;
            }

            var value = Values[ValueIndex++];

            setFieldValue(value.FieldInfo, arg);
        }

        private void validateThatRequiredFieldsAreFilled<TAttribute>(IEnumerable<Field<TAttribute>> fields) where TAttribute : KeyAttribute
        {
            foreach (var field in fields)
            {
                if (field.Attribute.IsRequired && field.FieldInfo.GetValue(Result) == null)
                {
                    Errors.Push($"Value for '{field.Attribute.Name}' is missing.");
                }
            }
        }

        internal Result<T> Parse()
        {
            while (Args.Count > 0 && Errors.Count == 0 && IsBreaked == false)
            {
                var arg = Args.Pop();

                if (!SkipKeys)
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

                    if (validateThatKeysAreParsed(arg))
                    {
                        continue;
                    }
                }

                parseValue(arg);
            }

            if (Errors.Count == 0 && IsBreaked == false)
            {
                validateThatRequiredFieldsAreFilled(Options.Values);
                validateThatRequiredFieldsAreFilled(Values);
            }

            return new Result<T>(Result, Errors, IsBreaked);
        }
    }

    public sealed class AppArgs : IAppArgs
    {
        internal static Field<TAttribute>[] CollectFields<TOptions, TAttribute>() where TAttribute : KeyAttribute
        {
            return typeof(TOptions)
                .GetFields()
                .Where(field => Attribute.IsDefined(field, typeof(TAttribute)))
                .Select(x => new Field<TAttribute>(x, x.GetCustomAttribute<TAttribute>()!))
                .OrderBy(x => x.Attribute.Order).ThenBy(x => x.FieldInfo.MetadataToken)
                .ToArray();
        }

        private static Dictionary<string, Field<T>> toFieldsByKeyMap<T>(Field<T>[] fields) where T : KeyAttribute
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

        public Result<T> Parse<T>(string[] args) where T : new()
        {
            var flagsFields = CollectFields<T, FlagAttribute>();
            var optionsFields = CollectFields<T, OptionAttribute>();
            var valuesFields = CollectFields<T, ValueAttribute>();

            var flagsByKey = toFieldsByKeyMap(flagsFields);
            var optionsByKey = toFieldsByKeyMap(optionsFields);

            var parser = new Parser<T>(args, flagsByKey, optionsByKey, valuesFields);

            return parser.Parse();
        }
    }
}
