using EasyApp.processor.components;

namespace EasyApp.processor
{
    internal interface IProcessorBuilder<TOptions>
    {
        IProcessor<TOptions> Build(ProcessorHandlers<TOptions> handlers, TOptions options);
    }

    internal sealed class ProcessorBuilder<TOptions> : IProcessorBuilder<TOptions>
    {
        private readonly EasyAppSettings Settings;

        public ProcessorBuilder(EasyAppSettings settings)
        {
            Settings = settings;
        }

        public IProcessor<TOptions> Build(ProcessorHandlers<TOptions> handlers, TOptions options)
        {
            var members = Reflector.CollectMembers<TOptions>();

            var logLevel = members.GetValue<LogLevelAttribute, LogLevel>(options, LogLevel.Normal);
            var console = new EasyAppConsole(logLevel, new AppInfoProvider(), Settings.Output.Encoding);

            return new Processor<TOptions>(console, handlers, members);
        }
    }
}
