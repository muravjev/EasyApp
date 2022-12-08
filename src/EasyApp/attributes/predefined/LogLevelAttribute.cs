namespace EasyApp
{

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
        public LogLevelAttribute(char shortKey = 'l', string longKey = "log-level", string description = "Logging level.", string valueName = "level", bool isRequired = true)
            : base(shortKey, longKey, description, valueName, isRequired) { }
    }
}
