namespace EasyApp.processor
{
    internal interface IProcessorHandler<TOptions>
    {
        int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console);
    }

    internal sealed class ProcessorHandlers<TOptions> : Dictionary<Type, IProcessorHandler<TOptions>> { }

    internal sealed class ProcessorHandler<TOptions> : IProcessorHandler<TOptions>
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
