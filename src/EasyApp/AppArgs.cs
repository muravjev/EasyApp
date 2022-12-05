﻿using System.ComponentModel;
using System.Reflection;

namespace EasyApp
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class FieldAttribute : Attribute
    {
        // Parameters order parsing! Order of Flags, Options, Parameters in Usage.
        public readonly int Order;

        // Parameter or Option <name>. Used in Usage and errors.
        public readonly string? Name;

        // Short key used in Flag and Option parsing and usage as -<key> /<key>.
        public readonly string? ShortKey;

        // Long key used in Flag and Option parsing and usage as --<key> /<key>.
        public readonly string? LongKey;

        // Description used in usage as description of Flag, Option or Parameter.
        public readonly string Description;

        // Used in validation of an Option and Parameter that value is not default.
        public readonly bool IsRequired;

        protected FieldAttribute(int order, string? name, string? shortKey, string? longKey, string description, bool isRequired)
        {
            Order = order;
            Name = name;
            ShortKey = shortKey;
            LongKey = longKey;
            Description = description;
            IsRequired = isRequired;
        }
    }

    public class FlagAttribute : FieldAttribute
    {
        public bool IsBreaker = true;

        public FlagAttribute(int order, char shortKey, string longKey, string description)
            : base(order, null, shortKey.ToString(), longKey, description, false) { }

        public FlagAttribute(char shortKey, string longKey, string description)
            : this(1, shortKey, longKey, description) { }

        public FlagAttribute(string longKey, string description)
            : base(1, longKey, null, longKey, description, false) { }
    }

    public class OptionAttribute : FieldAttribute
    {
        public OptionAttribute(int order, char shortKey, string longKey, string description, string? name = null, bool isRequired = true)
            : base(order, name, shortKey.ToString(), longKey, description, isRequired) { }

        public OptionAttribute(char shortKey, string longKey, string description, string? name = null, bool isRequired = true)
            : this(1, shortKey, longKey, description, name, isRequired) { }

        public OptionAttribute(int order, string longKey, string description, string? name = null, bool isRequired = true)
            : base(order, name, null, longKey, description, isRequired) { }

        public OptionAttribute(string longKey, string description, string? name = null, bool isRequired = true)
            : this(1, longKey, description, name, isRequired) { }
    }

    public class ParameterAttribute : FieldAttribute
    {
        public ParameterAttribute(int order, string name, string description, bool isRequired = true)
            : base(order, name, null, null, description, isRequired) { }

        public ParameterAttribute(string name, string description, bool isRequired = true)
            : this(1, name, description, isRequired) { }
    }

    public sealed class Result<T>
    {
        public readonly T Options;

        public readonly bool IsBreaked = false;

        public readonly Exception? Exception = null;

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

    internal record Field<TAttribute>(FieldInfo FieldInfo, TAttribute Attribute);

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
            Args = new Stack<string>(args.Reverse());
            Flags = flags;
            Options = options;
            Parameters = parameters;
        }

        private void setFieldValue(FieldInfo field, string value)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(field.FieldType);
                var converted = converter.ConvertFromInvariantString(value);

                field.SetValue(Result, converted);
            }
            catch (Exception e)
            {
                throw new AppException($"Failed to parse '{value}' as {field.FieldType.Name}.", e);
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

            flag.FieldInfo.SetValue(Result, true);

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

            setFieldValue(option.FieldInfo, value);
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

            setFieldValue(parameter.FieldInfo, arg);
        }

        private void validateThatRequiredFieldsAreFilled<TAttribute>(IEnumerable<Field<TAttribute>> fields) where TAttribute : FieldAttribute
        {
            foreach (var field in fields)
            {
                if (field.Attribute.IsRequired && field.FieldInfo.GetValue(Result) == null)
                {
                    throw new AppException($"Value for '{field.Attribute.Name}' is missing.");
                }
            }
        }

        internal void parse()
        {
            if (Args.Count == 0)
            {
                return;
            }

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

            validateThatRequiredFieldsAreFilled(Options.Values);
            validateThatRequiredFieldsAreFilled(Parameters);
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
        internal static Field<TAttribute>[] CollectFields<TOptions, TAttribute>() where TAttribute : FieldAttribute
        {
            return typeof(TOptions)
                .GetFields()
                .Where(field => Attribute.IsDefined(field, typeof(TAttribute)))
                .Select(x => new Field<TAttribute>(x, x.GetCustomAttribute<TAttribute>()!))
                .OrderBy(x => x.Attribute.Order).ThenBy(x => x.FieldInfo.MetadataToken)
                .ToArray();
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
            var flagsFields = CollectFields<T, FlagAttribute>();
            var optionsFields = CollectFields<T, OptionAttribute>();
            var parametersFields = CollectFields<T, ParameterAttribute>();

            var flagsByKey = toFieldsByKeyMap(flagsFields);
            var optionsByKey = toFieldsByKeyMap(optionsFields);

            var parser = new Parser<T>(args, flagsByKey, optionsByKey, parametersFields);

            return parser.Parse();
        }
    }
}
