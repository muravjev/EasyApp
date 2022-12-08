namespace EasyApp.processor
{
    public interface IProcessorHandler<TOptions>
    {
        int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console);
    }

    public sealed class ProcessorHandlers<TOptions> : Dictionary<Type, IProcessorHandler<TOptions>> { }

    public sealed class ProcessorHandler<TOptions> : IProcessorHandler<TOptions>
    {
        public readonly Func<EasyAppResult<TOptions>, IEasyAppConsole, int> Func;

        public ProcessorHandler(Func<EasyAppResult<TOptions>, IEasyAppConsole, int> func)
        {
            Func = func;
        }

        public int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console)
        {
            return Func(result, console);
        }
    }
}
