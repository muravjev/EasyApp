namespace EasyApp
{
    // Output Verbosity. Like https://learn.microsoft.com/en-us/visualstudio/ide/how-to-view-save-and-configure-build-log-files?view=vs-2022
    public enum Verbosity
    {
        Quiet,
        Minimal,
        Normal,
        Detailed,
        Debug
    }

    public class VerbosityAttribute : OptionAttribute
    {
        public VerbosityAttribute(char shortKey = default, string longKey = "verbosity", string description = "Logging level.", string valueName = "level", bool isRequired = true)
            : base(shortKey, longKey, description, valueName, isRequired) { }
    }
}
