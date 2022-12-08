namespace EasyApp.processor
{
    internal interface IProcessor<TOptions>
    {
        int Process(EasyAppResult<TOptions> result);
    }

    internal sealed class Processor<TOptions> : IProcessor<TOptions>
    {
        private readonly ProcessorHandlers<TOptions> Handlers;

        private readonly IEasyAppConsole Console;

        private readonly Member[] Members;

        public Processor(IEasyAppConsole console, ProcessorHandlers<TOptions> handlers, Member[] members)
        {
            Console = console;
            Handlers = handlers;
            Members = members;
        }

        private IProcessorHandler<TOptions> getOrCreate<T>(Func<IProcessorHandler<TOptions>> ctor)
        {
            return Handlers.GetValueOrDefault(typeof(T)) ?? ctor();
        }

        private IProcessorHandler<TOptions> getHandler(EasyAppResult<TOptions> result)
        {
            if (result.IsHelp)
            {
                return getOrCreate<HelpHandler<TOptions>>(() => new HelpHandler<TOptions>(Members));
            }

            if (result.Exception != null)
            {
                return getOrCreate<ExceptionHandler<TOptions>>(() => new ExceptionHandler<TOptions>());
            }

            var handler = Handlers.GetValueOrDefault(typeof(TOptions));
            if (handler == null)
            {
                throw new Exception("Process handler is not defined");
            }

            return handler;
        }

        public int Process(EasyAppResult<TOptions> result)
        {
            return getHandler(result).Handle(result, Console);
        }
    }
}
