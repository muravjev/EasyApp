namespace EasyApp
{
    public sealed class EasyAppResult<TOptions>
    {
        // Parse options.
        public readonly TOptions Options;

        // Whether execution is breaked and help is required (no args or help flag like --version).
        public readonly bool IsHelp = false;

        // Whether there were an exception during parsing.
        public readonly Exception? Error = null;

        // Whether parsing is succeesfull.
        public bool IsParsed => Error == null;

        public EasyAppResult(TOptions options, bool isHelp)
        {
            Options = options;
            IsHelp = isHelp;
        }

        public EasyAppResult(TOptions options, Exception error)
        {
            Options = options;
            Error = error;
        }
    }
}
