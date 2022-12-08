using EasyApp.processor;

namespace EasyApp
{
    public sealed class EasyApp<TOptions>
    {
        private readonly EasyAppSettings Settings = new EasyAppSettings();

        private readonly EasyAppHandlers<TOptions> Handlers = new EasyAppHandlers<TOptions>();

        private readonly IEasyAppParserBuilder<TOptions> ParserBuilder;

        private readonly IEasyAppProcessorBuilder<TOptions> ProcessorBuilder;

        public EasyApp(EasyAppSettings settings)
        {
            Settings = settings;
            ParserBuilder = new EasyAppParserBuilder<TOptions>();
            ProcessorBuilder = new EasyAppProcessorBuilder<TOptions>(settings);
        }

        public EasyApp() : this(new EasyAppSettings()) { }

        public EasyApp<TOptions> AddExceptionHandler(Func<Exception, int> handler)
        {
            Handlers.Add(typeof(ExceptionHandler<TOptions>), new EasyAppHandler<TOptions>((result, console) => handler(result.Exception!)));
            return this;
        }

        public EasyApp<TOptions> AddProcessHandler(Func<TOptions, int> handler)
        {
            Handlers.Add(typeof(TOptions), new EasyAppHandler<TOptions>((result, console) => handler(result.Options)));
            return this;
        }

        public EasyApp<TOptions> AddProcessHandler(Func<TOptions, IEasyAppConsole, int> handler)
        {
            Handlers.Add(typeof(TOptions), new EasyAppHandler<TOptions>((result, console) => handler(result.Options, console)));
            return this;
        }

        public EasyApp<TOptions> AddCommandHandler<T>(Func<TOptions, T, int> handler)
        {
            Handlers.Add(typeof(T), new EasyAppHandler<TOptions>((result, console) => handler(result.Options, (T)console)));
            return this;
        }

        public int Run(string[] args)
        {
            var parserResult = ParserBuilder.Build().Parse(args);
            return ProcessorBuilder.Build(Handlers, parserResult.Options).Process(parserResult);
        }
    }
}
