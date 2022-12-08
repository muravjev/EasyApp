using EasyApp.processor.components;

namespace EasyApp.processor
{
    internal static class ProcessorBuilder
    {
        public static IProcessor<TOptions> Build<TOptions>(EasyApp<TOptions> settings, Member[] members, TOptions options)
        {
            var logLevel = members.GetValue<LogLevelAttribute, LogLevel>(options, LogLevel.Normal);
            var console = new EasyAppConsole(logLevel, new AppInfoProvider(), settings.OutputEncoding);

            return new Processor<TOptions>(settings, console, members);
        }
    }
}
