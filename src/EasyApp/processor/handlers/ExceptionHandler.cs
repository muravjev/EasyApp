using System.Diagnostics;

namespace EasyApp.processor
{
    public sealed class ExceptionHandler<TOptions> : IEasyAppHandler<TOptions>
    {
        public int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console)
        {
            Debug.Assert(result != null);
            Debug.Assert(result.Exception != null);

            console.Exception(result.Exception!);
            console.Usage(result.Options);
            return -2;
        }
    }
}
