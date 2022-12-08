using EasyApp.processor.components;

namespace EasyApp.processor
{
    public interface IProcessor<TOptions>
    {
        int Process(EasyAppResult<TOptions> result);
    }

    public sealed class Processor<TOptions> : IProcessor<TOptions>
    {
        private readonly ProcessorHandlers<TOptions> Handlers;

        private readonly IEasyAppConsole Console;

        private readonly IValueFetcher<TOptions> ValueFetcher;

        public Processor(IEasyAppConsole console, ProcessorHandlers<TOptions> handlers, IValueFetcher<TOptions> valueFetcher)
        {
            Console = console;
            Handlers = handlers;
            ValueFetcher = valueFetcher;
        }

        private IProcessorHandler<TOptions> getOrCreate<T>(Func<IProcessorHandler<TOptions>> ctor)
        {
            return Handlers.GetValueOrDefault(typeof(T)) ?? ctor();
        }

        private IProcessorHandler<TOptions> getHandler(EasyAppResult<TOptions> result)
        {
            if (result.IsHelp)
            {
                return getOrCreate<HelpHandler<TOptions>>(() => new HelpHandler<TOptions>(ValueFetcher));
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
