using System.Diagnostics;

namespace EasyApp.processor
{
    internal sealed class ErrorHandler<TOptions> : IProcessorHandler<TOptions>
    {
        private readonly EasyAppSettings Settings;

        public ErrorHandler(EasyAppSettings settings)
        {
            Settings = settings;
        }

        public int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console)
        {
            Debug.Assert(result != null);
            Debug.Assert(result.Error != null);

            console.Exception(result.Error!);
            console.Usage(result.Options);

            return Settings.ParseErrorExitCode;
        }
    }
}
