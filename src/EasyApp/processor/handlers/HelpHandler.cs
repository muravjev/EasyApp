using EasyApp.processor.components;

namespace EasyApp.processor
{
    public sealed class HelpHandler<TOptions> : IProcessorHandler<TOptions>
    {
        private readonly IValueFetcher<TOptions> ValueFetcher;

        public HelpHandler(IValueFetcher<TOptions> valueFetcher)
        {
            ValueFetcher = valueFetcher;
        }

        public int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console)
        {
            if (ValueFetcher.Fetch<VersionAttribute>(result.Options))
            {
                console.Version();
                return 0;
            }

            // var isAll = getValue<AllAttribute, bool>(result.Options, false);

            if (ValueFetcher.Fetch<HelpAttribute>(result.Options))
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
