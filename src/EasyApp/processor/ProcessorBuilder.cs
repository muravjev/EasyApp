using EasyApp.processor.components;

namespace EasyApp.processor
{
    internal static class ProcessorBuilder
    {
        public static IProcessor<TOptions> Build<TOptions>(EasyApp<TOptions> settings, Member[] members, TOptions options)
        {
            var verbosity = members.GetValue<VerbosityAttribute, Verbosity>(options, Verbosity.Normal);
            var console = new EasyAppConsole<TOptions>(settings, members, verbosity, new AppInfoProvider());

            return new Processor<TOptions>(settings, console, members);
        }
    }
}
