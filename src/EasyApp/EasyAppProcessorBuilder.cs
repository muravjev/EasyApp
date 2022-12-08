using EasyApp.processor;

namespace EasyApp
{
    public interface IEasyAppProcessorBuilder<TOptions>
    {
        IEasyAppProcessor<TOptions> Build(EasyAppHandlers<TOptions> handlers, TOptions options);
    }

    public sealed class EasyAppProcessorBuilder<TOptions> : IEasyAppProcessorBuilder<TOptions>
    {
        private readonly EasyAppSettings Settings;

        public EasyAppProcessorBuilder(EasyAppSettings settings)
        {
            Settings = settings;
        }

        public IEasyAppProcessor<TOptions> Build(EasyAppHandlers<TOptions> handlers, TOptions options)
        {
            var members = Reflector.CollectMembers<TOptions>();
            var valueFetcher = new ValueFetcher<TOptions>(members);

            var logLevel = valueFetcher.Fetch<LogLevelAttribute, LogLevel>(options, LogLevel.Normal);
            var console = new EasyAppConsole(logLevel, new AppInfoProvider(), Settings.Output.Encoding);

            return new EasyAppProcessor<TOptions>(console, handlers, valueFetcher);
        }
    }
}
