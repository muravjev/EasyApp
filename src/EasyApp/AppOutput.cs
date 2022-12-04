using System.Reflection;
using System.Text;

namespace EasyApp
{
    public interface IAppOutput
    {
        void Header();

        void Version();

        void Description();

        void Usage<T>(T o);

        void Parameters<T>(T options);
    }

    public sealed class AppOutput : IAppOutput
    {
        private readonly IAppConsole Console;

        private readonly IAppInfo Info;

        public AppOutput(IAppConsole console, IAppInfo info)
        {
            Console = console;
            Info = info;
        }

        void IAppOutput.Header()
        {
            Console.WriteLine(LogLevel.Normal);
            Console.Write(LogLevel.Normal, Info.Title, ConsoleColor.White);
            Console.WriteLine(LogLevel.Normal, $", version {Info.Version}");
            Console.WriteLine(LogLevel.Normal, Info.Copyright);
        }

        void IAppOutput.Version()
        {
            Console.WriteLine(LogLevel.Normal, Info.Version);
        }

        void IAppOutput.Description()
        {
            Console.WriteLine(LogLevel.Normal);
            Console.WriteLine(LogLevel.Normal, Info.Description, ConsoleColor.Yellow);
        }

        private static string getOptionName(Field<OptionAttribute> field)
        {
            return field.Attribute.ValueName ?? field.FieldInfo.Name.ToLower();
        }

        private static int maxOrZero<TSource>(TSource[] source, Func<TSource, int> selector)
        {
            if (source.Length > 0)
            {
                return source.Max(selector);
            }

            return 0;
        }

        private string formatKey(KeyAttribute attribute, string? dataName = null)
        {
            var sb = new StringBuilder("  ");

            if (!string.IsNullOrEmpty(attribute.ShortKey))
            {
                sb.Append($"-{attribute.ShortKey}");

                if (!string.IsNullOrEmpty(attribute.LongKey))
                {
                    sb.Append(", ");
                }
            }
            else
            {
                sb.Append("    ");
            }

            if (!string.IsNullOrEmpty(attribute.LongKey))
            {
                sb.Append($"--{attribute.LongKey}");
            }

            if (!string.IsNullOrEmpty(dataName))
            {
                sb.Append($" <{dataName}>");
            }

            return sb.ToString();
        }

        private bool writeKey(LogLevel level, int maxWidth, string key)
        {

            if (key.Length > maxWidth - 2)
            {
                Console.WriteLine(level, key);
                Console.Write(level, new StringBuilder().Append(' ', maxWidth).ToString());
                return true;
            }

            Console.Write(level, key);
            Console.Write(level, new StringBuilder().Append(' ', maxWidth - key.Length).ToString());
            return false;
        }

        private bool writeValue(LogLevel level, string indent, string value)
        {
            var lines = value.Replace("\r", "").Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                if (i != 0)
                {
                    Console.Write(level, indent);
                }

                Console.WriteLine(level, lines[i]);
            }

            return lines.Length > 1;
        }

        private const int MIN_LEFT_COLUMN = 30;

        private void writeSection<T>(LogLevel loglevel, int maxWidth, string indent, Field<T>[] fields, string name, Func<Field<T>, string> formatKey, Func<KeyAttribute, FieldInfo, string> formatValue) where T : KeyAttribute
        {
            if (fields.Length > 0)
            {
                var multi = false;

                Console.Section(loglevel, name);

                foreach (var field in fields)
                {
                    if (multi)
                    {
                        Console.WriteLine(loglevel);
                    }

                    var key = formatKey(field);
                    multi = writeKey(loglevel, maxWidth, key);
                    var value = formatValue(field.Attribute, field.FieldInfo);
                    multi |= writeValue(loglevel, indent, value);
                }
            }
        }

        private void writeOptions<T>(LogLevel loglevel, Func<KeyAttribute, FieldInfo, string> formatValue)
        {
            var flagsFields = AppArgs.CollectFields<T, FlagAttribute>();
            var optionsFields = AppArgs.CollectFields<T, OptionAttribute>();
            var parametersFields = AppArgs.CollectFields<T, ValueAttribute>();

            var flagsMaxWidth = maxOrZero(flagsFields, x => x.Attribute.LongKey?.Length ?? 0) + "  - , --  ".Length;
            var optionsMaxWidth = maxOrZero(optionsFields, x => (x.Attribute.LongKey?.Length ?? 0) + getOptionName(x).Length + "  - , -- <>  ".Length);
            var parametersMaxWidth = maxOrZero(parametersFields, x => x.Attribute.Name.Length + "  <>  ".Length);
            var maxWidth = Math.Min(MIN_LEFT_COLUMN, Math.Max(Math.Max(flagsMaxWidth, optionsMaxWidth), parametersMaxWidth));

            var indent = new StringBuilder().Append(' ', maxWidth).ToString();

            writeSection(loglevel, maxWidth, indent, flagsFields, "Flags", f => formatKey(f.Attribute), formatValue);
            writeSection(loglevel, maxWidth, indent, optionsFields, "Options", f => formatKey(f.Attribute, getOptionName(f).ToLower()), formatValue);
            writeSection(loglevel, maxWidth, indent, parametersFields, "Parameters", f => $"  <{f.Attribute.Name}>", formatValue);
        }

        private string? getValue<T>(FieldInfo fi, T options)
        {
            var value = fi.GetValue(options)?.ToString();

            if (fi.FieldType == typeof(string))
            {
                return value;
            }

            return value?.ToLower();
        }

        void IAppOutput.Usage<T>(T options)
        {
            Console.WriteLine(LogLevel.Normal);
            Console.Write(LogLevel.Normal, "Usage: ");
            Console.Write(LogLevel.Normal, Info.Product);

            var flagsFields = AppArgs.CollectFields<T, FlagAttribute>();
            var optionsFields = AppArgs.CollectFields<T, OptionAttribute>();
            var parametersFields = AppArgs.CollectFields<T, ValueAttribute>();

            if (flagsFields.Length > 0)
            {
                Console.Write(LogLevel.Normal, " [flags...]");
            }

            if (optionsFields.Length > 0)
            {
                Console.Write(LogLevel.Normal, " [options...]");
            }

            foreach (var parameter in parametersFields)
            {
                Console.Write(LogLevel.Normal, $" <{parameter.Attribute.Name}>");
            }

            Console.WriteLine(LogLevel.Normal);

            writeOptions<T>(LogLevel.Normal, (attr, fi) =>
            {
                var sb = new StringBuilder();

                sb.Append(attr.Description);

                if (fi.FieldType.IsEnum)
                {
                    sb.AppendLine();
                    sb.Append($"Values: {string.Join("|", Enum.GetNames(fi.FieldType).Select(x => x.ToLower()))}.");
                }

                if (fi.FieldType != typeof(bool))
                {
                    var value = getValue(fi, options);
                    if (value != null)
                    {
                        sb.AppendLine();
                        sb.Append($"Default: {value}.");
                    }
                }

                return sb.ToString();

            });
        }

        void IAppOutput.Parameters<T>(T options)
        {
            writeOptions<T>(LogLevel.Debug, (attr, fi) =>
            {
                return getValue(fi, options) ?? "<null>";
            });
        }
    }
}
