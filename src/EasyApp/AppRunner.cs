namespace EasyApp
{
    public sealed class HelpAttribute : FlagAttribute
    {
        public HelpAttribute(int order = 0, char shortKey = 'h', string longKey = "help", string description = "Display help.")
            : base(order, shortKey, longKey, description, true) { }
    }

    public sealed class VersionAttribute : FlagAttribute
    {
        public VersionAttribute(int order = 0, char shortKey = 'v', string longKey = "version", string description = "Display version information.")
            : base(order, shortKey, longKey, description, true) { }
    }

    public sealed class AllAttribute : FlagAttribute
    {
        public AllAttribute(int order = 0, char shortKey = default, string longKey = "all", string description = "Display all options.")
            : base(order, shortKey, longKey, description, true) { }
    }
    public sealed class VerboseAttribute : FlagAttribute
    {
        public VerboseAttribute(int order = 0, char shortKey = default, string longKey = "verbose", string description = "Turn on verbose logging.")
            : base(order, shortKey, longKey, description) { }
    }

    public sealed class QuietAttribute : FlagAttribute
    {
        public QuietAttribute(int order = 0, char shortKey = default, string longKey = "quiet", string description = "Turn off logging.")
            : base(order, shortKey, longKey, description) { }
    }

    public enum LogLevel
    {
        Quiet,
        Error,
        Normal,
        Verbose,
        Debug
    }

    public class LogLevelAttribute : OptionAttribute
    {
        public LogLevelAttribute(int order = 0, char shortKey = 'l', string longKey = "log-level", string description = "Logging level.", string valueName = "level", bool isRequired = true)
            : base(order, shortKey, longKey, description, valueName, isRequired) { }
    }

    public abstract class OptionsBase
    {
        [Help]
        public bool Help = false;

        [Version]
        public bool Version = false;

        [Verbose]
        public bool Verbose = false;

        [Quiet]
        public bool Queit = false;

        [LogLevel]
        public LogLevel LogLevel = LogLevel.Normal;
    }

    public interface IAppRunner
    {
        int Run(string[] args);
    }

    public sealed class AppRunner<TOptions, TProcessor> : IAppRunner
        where TOptions : new()
        where TProcessor : IAppProcessor
    {
        private readonly IAppArgs Args;

        private readonly IAppConsoleMaster Console;

        private readonly IAppOutput Output;

        public AppRunner()
        {
            Args = new AppArgs();
            Console = new AppConsole(LogLevel.Normal);
            Output = new AppOutput(Console, new AppInfo());
        }

        private static TValue getValue<TAttribute, TValue>(TOptions options, TValue defaultValue) where TAttribute : Attribute
        {
            var field = typeof(TOptions)
                .GetFields()
                .Where(field => Attribute.IsDefined(field, typeof(TAttribute)))
                .FirstOrDefault();

            if (field == null)
            {
                return defaultValue;
            }

            if (field.FieldType != typeof(TValue))
            {
                return defaultValue;
            }

            var value = field.GetValue(options);
            if (value == null)
            {
                return defaultValue;
            }

            return (TValue)value;
        }

        private TProcessor? createProcessor(TOptions options)
        {
            var ctor = typeof(TProcessor).GetConstructor(new Type[] { typeof(TOptions), typeof(IAppConsole) });
            if (ctor != null)
            {
                return (TProcessor)ctor.Invoke(new object[] { options!, Console });
            }

            ctor = typeof(TProcessor).GetConstructor(new Type[] { typeof(TOptions) });
            if (ctor != null)
            {
                return (TProcessor)ctor.Invoke(new object[] { options! });
            }

            return default;
        }

        public int Run(string[] args)
        {
            try
            {
                var result = Args.Parse<TOptions>(args);

                Console.Setup(getValue<LogLevelAttribute, LogLevel>(result.Options, LogLevel.Normal));

                if (result.IsBreaked)
                {
                    if (getValue<VersionAttribute, bool>(result.Options, false))
                    {
                        Output.Version();
                        return 0;
                    }

                    if (getValue<HelpAttribute, bool>(result.Options, false))
                    {
                        Output.Usage(result.Options);
                        return 0;
                    }

                    Output.Header();
                    Output.Description();
                    Output.Usage(result.Options);
                    return 0;
                }

                if (result.Exception != null)
                {
                    Console.Exception(result.Exception);
                    Output.Usage(result.Options);
                    return -2;
                }

                Output.Header();
                Output.Parameters(result.Options);

                var processor = createProcessor(result.Options);
                if (processor == null)
                {
                    Console.Error($"Error creating processor ({typeof(TProcessor).FullName}).");
                    Console.WriteLine(LogLevel.Error, $"Processor should have public constructor.");
                    return -3;
                }

                return processor.Process();
            }
            catch (Exception e)
            {
                Console.Exception(e);
                return -1;
            }
        }
    }
}
