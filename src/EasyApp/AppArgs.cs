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
                }

                if (!string.IsNullOrEmpty(attr.LongKey))
                {
                    byKey.Add($"--{attr.LongKey}", field);
                }
            }

            return byKey;
        }

        private static bool setFieldValue<T>(T result, FieldInfo field, string value, Stack<string> errors)
        {
            if (field.FieldType == typeof(string))
            {
                field.SetValue(result, value);
                return true;
            }
            else if (field.FieldType == typeof(int))
            {
                if (int.TryParse(value, out int intValue))
                {
                    field.SetValue(result, intValue);
                    return true;
                }
                else
                {
                    errors.Push($"Failed to parse '{value}' as int.");
                }
            }
            else if (field.FieldType.IsEnum)
            {
                var enumValue = Enum.Parse(field.FieldType, value, true);
                field.SetValue(result, enumValue);
                return true;
            }
            else
            {
                errors.Push($"Unsupported type '{field.FieldType.Name}'.");
            }

            return false;
        }

        private static void validateRequiredFields<T, TAttribute>(T result, Field<TAttribute>[] fields, Stack<string> errors) where TAttribute : KeyAttribute
        {
            foreach (var field in fields)
            {
                if (field.Attribute.IsRequired && field.FieldInfo.GetValue(result) == null)
                {
                    errors.Push($"Value for '{field.Attribute.Name}' is missing.");
                }
            }
        }

        Result<T> IAppArgs.Parse<T>(string[] args)
        {
            var result = new T();
            var errors = new Stack<string>();

            if (args.Length == 0)
            {
                return new Result<T>(result, errors, true);
            }

            var flagsFields = CollectFields<T, FlagAttribute>();
            var optionsFields = CollectFields<T, OptionAttribute>();
            var valuesFields = CollectFields<T, ValueAttribute>();

            var flagsByKey = toFieldsByKeyMap(flagsFields);
            var optionsByKey = toFieldsByKeyMap(optionsFields);

            var valueIndex = 0;
            var skipKeys = false;

            for (var i = 0; i < args.Length; ++i)
            {
                var arg = args[i];

                if (skipKeys == false)
                {
                    if (arg == "--")
                    {
                        skipKeys = true;
                        continue;
                    }

                    var flag = flagsByKey.GetValueOrDefault(arg);
                    if (flag != null)
                    {
                        flag.FieldInfo.SetValue(result, true);

                        if (flag.Attribute.IsBreaker)
                        {
                            return new Result<T>(result, errors, true);
                        }

                        continue;
                    }

                    var option = optionsByKey.GetValueOrDefault(arg);
                    if (option != null)
                    {
                        if (++i >= args.Length)
                        {
                            errors.Push($"Missing option '{option.Attribute.Name}' value '{arg}'");
                            break;
                        }

                        var optionValue = args[i];

                        if (!setFieldValue(result, option.FieldInfo, optionValue, errors))
                        {
                            break;
                        }

                        continue;
                    }

                    if (arg.StartsWith("-"))
                    {
                        errors.Push($"Unknown key '{arg}'.");
                        break;
                    }
                }

                if (valueIndex >= valuesFields.Length)
                {
                    errors.Push($"Unexpected parameter '{arg}'");
                    break;
                }

                var value = valuesFields[valueIndex];

                if (!setFieldValue(result, value.FieldInfo, arg, errors))
                {
                    break;
                }

                valueIndex++;
            }

            if (errors.Count == 0)
            {
                validateRequiredFields(result, optionsFields, errors);
                validateRequiredFields(result, valuesFields, errors);
            }

            return new Result<T>(result, errors, false);
        }
    }
}
