namespace EasyApp
{
    public static class EasyAppDemo
    {
        public sealed class Options { }

        public sealed class Command { }

        private static int process(Options options)
        {
            return 0;
        }

        private static int process(Options options, Command command)
        {
            return 0;
        }

        public static int Main(string[] args)
        {
            return new EasyApp<Options>()
                .AddProcessHandler(process)
                .AddCommandHandler<Command>(process)
                .AddErrorHandler(e => -1)
                .Run(args);
        }
    }
}
