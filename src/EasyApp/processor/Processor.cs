namespace EasyApp.processor
{
    internal interface IProcessor<TOptions>
    {
        int Process(EasyAppResult<TOptions> result);
    }

    internal sealed class Processor<TOptions> : IProcessor<TOptions>
    {
        private readonly EasyApp<TOptions> Settings;

        private readonly IEasyAppConsole Console;

        private readonly Member[] Members;

        public Processor(EasyApp<TOptions> settings, IEasyAppConsole console, Member[] members)
        {
            Settings = settings;
            Console = console;
            Members = members;
        }

        private IProcessorHandler<TOptions>? get<T>()
        {
            return Settings.Handlers.GetValueOrDefault(typeof(T));
        }

        private IProcessorHandler<TOptions> getOrCreate<T>(Func<IProcessorHandler<TOptions>> ctor)
        {
            return get<T>() ?? ctor();
        }

        private IProcessorHandler<TOptions> getHandler(EasyAppResult<TOptions> result)
        {
            if (result.IsHelp)
            {
                return getOrCreate<HelpHandler<TOptions>>(() => new HelpHandler<TOptions>(Settings, Members));
            }

            if (result.Error != null)
            {
                return getOrCreate<ErrorHandler<TOptions>>(() => new ErrorHandler<TOptions>(Settings));
            }

            var handler = get<TOptions>();
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
