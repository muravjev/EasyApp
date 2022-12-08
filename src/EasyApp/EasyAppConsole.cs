using EasyApp.processor.components;
using System.Text;

namespace EasyApp
{
    public interface IEasyAppConsole
    {
        void Write(LogLevel level, string? message = null, ConsoleColor? color = null);

        void WriteLine(LogLevel level, string? message = null, ConsoleColor? color = null);

        void Section(LogLevel level, string name);

        void Error(string message);

        void Exception(Exception e);

        void Header();

        void Version();

        void Description();

        void Usage<T>(T options);

        void Parameters<T>(T options);
    }

    internal class EasyAppConsole : IEasyAppConsole
    {
        private readonly LogLevel Level;

        private readonly IAppInfoProvider Info;

        public EasyAppConsole(LogLevel level, IAppInfoProvider info, Encoding? encoding)
        {
            Level = level;
            Info = info;

            if (encoding != null)
            {
                Console.OutputEncoding = encoding;
            }
        }

        private void output(LogLevel level, string? message, ConsoleColor? color, Action<string?> cb)
        {
            if (Level >= level)
            {
                if (color == null)
                {
                    cb(message);
                    return;
                }

                var save = Console.ForegroundColor;

                try
                {
                    Console.ForegroundColor = (ConsoleColor)color;

                    cb(message);
                }
                finally
                {
                    Console.ForegroundColor = save;
                }
            }
        }

        public void Write(LogLevel level, string? message = null, ConsoleColor? color = null)
        {
            output(level, message, color, Console.Write);
        }

        public void WriteLine(LogLevel level, string? message = null, ConsoleColor? color = null)
        {
            output(level, message, color, Console.WriteLine);
        }

        public void Section(LogLevel level, string name)
        {
            WriteLine(level);
            WriteLine(level, $"{name}:");
            WriteLine(level);
        }

        public void Error(string message)
        {
            WriteLine(LogLevel.Error);
            Write(LogLevel.Error, "Error: ");
            WriteLine(LogLevel.Error, message, ConsoleColor.Red);
        }

        public void Exception(Exception e)
        {
            for (var ex = e; ex != null; ex = ex.InnerException)
            {
                WriteLine(LogLevel.Error);

                if (ex == e)
                {
                    Write(LogLevel.Error, "Exception: ");
                }
                else
                {
                    Write(LogLevel.Error, "--> InnerException: ");
                }

                WriteLine(LogLevel.Error, ex.Message, ConsoleColor.Red);

                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    WriteLine(LogLevel.Verbose, ex.StackTrace);
                }
            }
        }

        public void Header()
        {
            WriteLine(LogLevel.Normal);
            Write(LogLevel.Normal, Info.Title, ConsoleColor.White);
            WriteLine(LogLevel.Normal, $", version {Info.Version}");
            WriteLine(LogLevel.Normal, Info.Copyright);
        }

        public void Version()
        {
            WriteLine(LogLevel.Normal, Info.Version);
        }

        public void Description()
        {
            WriteLine(LogLevel.Normal);
            WriteLine(LogLevel.Normal, Info.Description, ConsoleColor.Yellow);
        }

        //        private static int maxOrZero<TSource>(TSource[] source, Func<TSource, int> selector)
        //        {
        //            if (source.Length > 0)
        //            {
        //                return source.Max(selector);
        //            }

        //            return 0;
        //        }

        //        private bool writeKey(LogLevel level, int maxWidth, string key)
        //        {
        //            if (key.Length > maxWidth - 2)
        //            {
        //                Console.WriteLine(level, key);
        //                Console.Write(level, new StringBuilder().Append(' ', maxWidth).ToString());
        //                return true;
        //            }

        //            Console.Write(level, key);
        //            Console.Write(level, new StringBuilder().Append(' ', maxWidth - key.Length).ToString());
        //            return false;
        //        }

        //        private bool writeValue(LogLevel level, string indent, string value)
        //        {
        //            var lines = value.Replace("\r", "").Split('\n');

        //            for (var i = 0; i < lines.Length; i++)
        //            {
        //                if (i != 0)
        //                {
        //                    Console.Write(level, indent);
        //                }

        //                Console.WriteLine(level, lines[i]);
        //            }

        //            return lines.Length > 1;
        //        }

        //        private const int MIN_LEFT_COLUMN = 30;

        //        private static string getName(Member member)
        //        {
        //            return member.Attribute.Name ?? member.Name.ToLower();
        //        }

        //        private static string getKey(Member member)
        //        {
        //            var attribute = member.Attribute;

        //            if (!string.IsNullOrEmpty(attribute.ShortKey) && !string.IsNullOrEmpty(attribute.LongKey))
        //            {
        //                return $"-{attribute.ShortKey}, --{attribute.LongKey}";
        //            }

        //            if (!string.IsNullOrEmpty(attribute.ShortKey))
        //            {
        //                return $"-{attribute.ShortKey}";
        //            }

        //            return $"    --{attribute.LongKey}";
        //        }

        //        private string formatKey(Member member)
        //        {
        //            var type = member.Attribute.GetType();

        //            if (typeof(FlagAttribute).IsAssignableFrom(type))
        //            {
        //                return $"  {getKey(member)}";
        //            }

        //            if (typeof(OptionAttribute).IsAssignableFrom(type))
        //            {
        //                return $"  {getKey(member)} <{getName(member)}>";
        //            }

        //            if (typeof(ParameterAttribute).IsAssignableFrom(type))
        //            {
        //                return $"  <{getName(member)}>";
        //            }

        //            throw new Exception($"Unexpected attribute {type.FullName}");
        //        }

        //        private void writeSection(LogLevel loglevel, int maxWidth, string indent, Member[] members, string name, Func<Member, string> formatValue)
        //        {
        //            if (members.Length > 0)
        //            {
        //                var multi = false;

        //                Console.Section(loglevel, name);

        //                foreach (var member in members)
        //                {
        //                    if (multi)
        //                    {
        //                        Console.WriteLine(loglevel);
        //                    }

        //                    var key = formatKey(member);
        //                    multi = writeKey(loglevel, maxWidth, key);
        //                    var value = formatValue(member);
        //                    multi |= writeValue(loglevel, indent, value);
        //                }
        //            }
        //        }

        //        private void writeOptions<T>(LogLevel loglevel, Func<Member, string> formatValue)
        //        {
        //            var members = AppReflector.CollectMembers<T>();

        //            var flagsFields = members.Filter(MemberType.Flag);
        //            var optionsFields = members.Filter(MemberType.Option);
        //            var parametersFields = members.Filter(MemberType.Parameter);

        //            var flagsMaxWidth = maxOrZero(flagsFields, x => x.Attribute.LongKey?.Length ?? 0) + "  - , --  ".Length;
        //            var optionsMaxWidth = maxOrZero(optionsFields, x => (x.Attribute.LongKey?.Length ?? 0) + getName(x).Length + "  - , -- <>  ".Length);
        //            var parametersMaxWidth = maxOrZero(parametersFields, x => getName(x).Length + "  <>  ".Length);
        //            var maxWidth = Math.Min(MIN_LEFT_COLUMN, Math.Max(Math.Max(flagsMaxWidth, optionsMaxWidth), parametersMaxWidth));

        //            var indent = new StringBuilder().Append(' ', maxWidth).ToString();

        //            writeSection(loglevel, maxWidth, indent, flagsFields, "Flags", formatValue);
        //            writeSection(loglevel, maxWidth, indent, optionsFields, "Options", formatValue);
        //            writeSection(loglevel, maxWidth, indent, parametersFields, "Parameters", formatValue);
        //        }

        //        private string? getValue<T>(Member member, T options)
        //        {
        //            var value = member.GetValue(options)?.ToString();

        //            if (member.Type == typeof(string))
        //            {
        //                return value;
        //            }

        //            return value?.ToLower();
        //        }


        public void Usage<T>(T options)
        {
            WriteLine(LogLevel.Normal, "USAGE");

            //            Console.WriteLine(LogLevel.Normal);
            //            Console.Write(LogLevel.Normal, "Usage: ");
            //            Console.Write(LogLevel.Normal, Info.Product);

            //            var members = AppReflector.CollectMembers<T>();

            //            var flagsFields = members.Filter(MemberType.Flag);
            //            var optionsFields = members.Filter(MemberType.Option);
            //            var parametersFields = members.Filter(MemberType.Parameter);

            //            if (flagsFields.Length > 0)
            //            {
            //                Console.Write(LogLevel.Normal, " [flags...]");
            //            }

            //            if (optionsFields.Length > 0)
            //            {
            //                Console.Write(LogLevel.Normal, " [options...]");
            //            }

            //            foreach (var parameter in parametersFields)
            //            {
            //                Console.Write(LogLevel.Normal, $" <{parameter.Attribute.Name}>");
            //            }

            //            Console.WriteLine(LogLevel.Normal);

            //            writeOptions<T>(LogLevel.Normal, (member) =>
            //            {
            //                var sb = new StringBuilder();

            //                sb.Append(member.Attribute.Description);

            //                if (member.Type.IsEnum)
            //                {
            //                    sb.AppendLine();
            //                    sb.Append($"Values: {string.Join("|", Enum.GetNames(member.Type).Select(x => x.ToLower()))}.");
            //                }

            //                if (member.Type != typeof(bool))
            //                {
            //                    var value = getValue(member, options);
            //                    if (value != null)
            //                    {
            //                        sb.AppendLine();
            //                        sb.Append($"Default: {value}.");
            //                    }
            //                }

            //                return sb.ToString();

            //            });

        }

        public void Parameters<T>(T options)
        {
            WriteLine(LogLevel.Normal, "PARAMETERS");


            //            writeOptions<T>(LogLevel.Debug, (member) =>
            //            {
            //                return getValue(member, options) ?? "<null>";
            //            });
        }
    }
}
