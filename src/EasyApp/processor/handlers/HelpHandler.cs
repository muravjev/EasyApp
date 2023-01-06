namespace EasyApp.processor
{
    internal sealed class HelpHandler<TOptions> : IProcessorHandler<TOptions>
    {
        private readonly EasyAppSettings Settings;

        private readonly Member[] Members;

        public HelpHandler(EasyAppSettings settings, Member[] members)
        {
            Settings = settings;
            Members = members;
        }

        public int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console)
        {
            if (Members.GetValue<VersionAttribute>(result.Options))
            {
                console.Version();
                return 0;
            }

            if (Members.GetValue<HelpAttribute>(result.Options))
            {
                console.Usage(result.Options);
                return 0;
            }

            console.Header();
            console.Description();
            console.Usage(result.Options);
            return 0;
        }
    }
}
