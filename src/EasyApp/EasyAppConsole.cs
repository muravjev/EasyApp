using EasyApp.processor.components;
using System.Text;

namespace EasyApp
{
    public interface IEasyAppConsole
    {
        void Write(Verbosity verbosity, string? message = null, ConsoleColor? color = null);

        void WriteLine(Verbosity verbosity, string? message = null, ConsoleColor? color = null);

        void Section(Verbosity verbosity, string name);

        void Error(string message);

        void Exception(Exception e);

        void Header();

        void Version();

        void Description();

        void Usage<T>(T options);

        void Parameters<T>(T options);
    }

    internal sealed class EasyAppConsole<TOptions> : IEasyAppConsole
    {
        private readonly EasyAppSettings Settings;

        private readonly Member[] Members;

        private readonly Verbosity Verbosity;

        private readonly IAppInfoProvider Info;

        public EasyAppConsole(EasyAppSettings settings, Member[] members, Verbosity verbosity, IAppInfoProvider info)
        {
            Settings = settings;
            Members = members;
            Verbosity = verbosity;
            Info = info;

            if (settings.OutputEncoding != null)
            {
                Console.OutputEncoding = settings.OutputEncoding;
            }
        }

        private void write(Verbosity verbosity, string? message, ConsoleColor? color, Action<string?> cb)
        {
            if (Verbosity >= verbosity)
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

        public void Write(Verbosity verbosity, string? message = null, ConsoleColor? color = null)
        {
            write(verbosity, message, color, Console.Write);
        }

        public void WriteLine(Verbosity verbosity, string? message = null, ConsoleColor? color = null)
        {
            write(verbosity, message, color, Console.WriteLine);
        }

        public void Section(Verbosity verbosity, string name)
        {
            WriteLine(verbosity);
            WriteLine(verbosity, $"{name}:");
            WriteLine(verbosity);
        }

        public void Error(string message)
        {
            WriteLine(Verbosity.Minimal);
            Write(Verbosity.Minimal, "Error: ");
            WriteLine(Verbosity.Minimal, message, ConsoleColor.Red);
        }

        public void Exception(Exception e)
        {
            for (var ex = e; ex != null; ex = ex.InnerException)
            {
                WriteLine(Verbosity.Minimal);

                if (ex == e)
                {
                    Write(Verbosity.Minimal, "Exception: ");
                }
                else
                {
                    Write(Verbosity.Minimal, "--> InnerException: ");
                }

                WriteLine(Verbosity.Minimal, ex.Message, ConsoleColor.Red);

                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    WriteLine(Verbosity.Detailed, ex.StackTrace);
                }
            }
        }

        public void Header()
        {
            WriteLine(Verbosity.Normal);
            Write(Verbosity.Normal, Info.Title, ConsoleColor.White);
            WriteLine(Verbosity.Normal, $", version {Info.Version}");
            WriteLine(Verbosity.Normal, Info.Copyright);
        }

        public void Version()
        {
            WriteLine(Verbosity.Normal, Info.Version);
        }

        public void Description()
        {
            WriteLine(Verbosity.Normal);
            WriteLine(Verbosity.Normal, Info.Description, ConsoleColor.Yellow);
        }

        private void writeSections(Verbosity verbosity, GroupsData groups)
        {
            var indent = new StringBuilder().Append(' ', groups.UsageSectionIndentLength).Append(Settings.UsageSectionKeyPosfix).ToString();

            foreach (var group in groups.Groups)
            {
                Section(verbosity, group.Attribute.Name);

                foreach (var member in group.Members)
                {
                    if (Settings.UsageSectionAddLineForMultiLine == MultiLine.Always ||
                        (Settings.UsageSectionAddLineForMultiLine == MultiLine.Auto && group.SectionIndentIsCrossed))
                    {
                        WriteLine(verbosity);
                    }

                    Write(verbosity, member.key);

                    var append = groups.UsageSectionIndentLength - member.key.Length;

                    if (append >= 0)
                    {
                        Write(verbosity, new StringBuilder().Append(' ', append).ToString());
                        Write(verbosity, Settings.UsageSectionKeyPosfix);
                    }
                    else
                    {
                        WriteLine(verbosity);
                        Write(verbosity, indent);
                    }

                    WriteLine(verbosity, member.member.Attribute.Description);
                }
            }
        }

        public void Usage<T>(T options)
        {
            WriteLine(Verbosity.Normal);
            Write(Verbosity.Normal, "Usage: ");
            Write(Verbosity.Normal, Info.Product);

            var showHidden = Members.GetValue<AllAttribute>(options);
            var groups = Members.ToGroups(showHidden);
            var groupsData = groups.ToGroupsData(Settings);

            foreach (var group in groups)
            {
                if (group.Attribute.Expandable &&
                    group.Members.Length < 4 &&
                    group.Members.All(m => m.Attribute.Type == MemberType.Flag || m.Attribute.Type == MemberType.Parameter))
                {
                    foreach (var member in group.Members)
                    {
                        Write(Verbosity.Normal, member.ToInlineUsage(Settings));
                    }
                }
                else
                {
                    Write(Verbosity.Normal, $" [{group.Attribute.Name.ToLower()}...]");
                }
            }

            WriteLine(Verbosity.Normal);
            writeSections(Verbosity.Normal, groupsData);
        }

        public void Parameters<T>(T options)
        {
            WriteLine(Verbosity.Normal, "PARAMETERS");

            // writeOptions<T>(LogLevel.Debug, (member) =>
            // {
            //     return getValue(member, options) ?? "<null>";
            // });
        }
    }
}
