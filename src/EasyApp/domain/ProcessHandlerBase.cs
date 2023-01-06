namespace EasyApp
{
    public interface IProcessHandler
    {
        int Process();
    }

    public abstract class ProcessHandlerBase<TOptions> : IProcessHandler
    {
        public readonly TOptions Options;

        public readonly IEasyAppConsole Console;

        protected ProcessHandlerBase(TOptions options, IEasyAppConsole console)
        {
            Options = options;
            Console = console;
        }

        public abstract int Process();
    }
}
