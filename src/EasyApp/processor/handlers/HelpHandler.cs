namespace EasyApp.processor
{
    internal sealed class HelpHandler<TOptions> : IProcessorHandler<TOptions>
    {
        private readonly Member[] Members;

        public HelpHandler(Member[] members)
        {
            Members = members;
        }

        public int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console)
        {
            if (Members.GetValue<VersionAttribute>(result.Options))
            {
                console.Version();
                return 0;
            }

            // var isAll = GetValue<VersionAttribute>(result.Options);

            if (Members.GetValue<VersionAttribute>(result.Options))
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
