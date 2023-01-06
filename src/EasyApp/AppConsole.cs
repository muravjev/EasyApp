namespace EasyApp
{
    public interface IAppConsole
    {
        void Write(LogLevel level, string? message = null, ConsoleColor? color = null);

        void WriteLine(LogLevel level, string? message = null, ConsoleColor? color = null);

        void Section(LogLevel level, string name);

        void Error(string message);

        void Exception(Exception e);
    }

    internal interface IAppConsoleMaster : IAppConsole
    {
        void Setup(LogLevel level);
    }

    internal class AppConsole : IAppConsole, IAppConsoleMaster
    {
        private LogLevel Level;

        public AppConsole(LogLevel level)
        {
            Level = level;
            Console.OutputEncoding = System.Text.Encoding.Unicode;
        }

        void IAppConsoleMaster.Setup(LogLevel level)
        {
            Level = level;
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

        void IAppConsole.Section(LogLevel level, string name)
        {
            WriteLine(level);
            WriteLine(level, $"{name}:");
            WriteLine(level);
        }

        void IAppConsole.Error(string message)
        {
            WriteLine(LogLevel.Error);
            Write(LogLevel.Error, "Error: ");
            WriteLine(LogLevel.Error, message, ConsoleColor.Red);
        }

        void IAppConsole.Exception(Exception e)
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
    }
}
