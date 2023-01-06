namespace EasyApp
{
    public static class EasyAppDemo
    {
        public sealed class Options
        {
            [Command("add", "Adds support")]
            public readonly AddCommand? command1;
        }

        public sealed class AddCommand { }

        private static int process(Options options)
        {
            return 0;
        }

        private static int process(Options options, AddCommand command)
        {
            return 0;
        }

        public static int Main(string[] args)
        {
            return new EasyApp<Options>()
                .AddProcessHandler(process)
                .AddCommandHandler<AddCommand>(process)
                .AddErrorHandler(e => -1)
                .Run(args);
        }
    }
}
