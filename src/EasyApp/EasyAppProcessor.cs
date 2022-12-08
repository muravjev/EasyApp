using EasyApp.processor;

namespace EasyApp
{
    public interface IEasyAppProcessor<TOptions>
    {
        int Process(EasyAppResult<TOptions> result);
    }

    public sealed class EasyAppProcessor<TOptions> : IEasyAppProcessor<TOptions>
    {
        private readonly EasyAppHandlers<TOptions> Handlers;

        private readonly IEasyAppConsole Console;

        private readonly IValueFetcher<TOptions> ValueFetcher;

        public EasyAppProcessor(IEasyAppConsole console, EasyAppHandlers<TOptions> handlers, IValueFetcher<TOptions> valueFetcher)
        {
            Console = console;
            Handlers = handlers;
            ValueFetcher = valueFetcher;
        }

        private IEasyAppHandler<TOptions> getOrCreate<T>(Func<IEasyAppHandler<TOptions>> ctor)
        {
            return Handlers.GetValueOrDefault(typeof(T)) ?? ctor();
        }

        private IEasyAppHandler<TOptions> getHandler(EasyAppResult<TOptions> result)
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
