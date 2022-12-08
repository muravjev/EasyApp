using EasyApp.parser;
using EasyApp.processor;
using System.Text;

namespace EasyApp
{
    public sealed class EasyApp<TOptions>
    {
        public int UnhandledExceptionExitCode = -1;
        public int ParseErrorExitCode = -2;

        public Encoding OutputEncoding = Encoding.Unicode;

        #region Handlers

        internal readonly ProcessorHandlers<TOptions> Handlers = new ProcessorHandlers<TOptions>();

        public EasyApp<TOptions> AddErrorHandler(Func<Exception, int> handler)
        {
            Handlers.Add(typeof(ErrorHandler<TOptions>), new ProcessorHandler<TOptions>((result, console) => handler(result.Error!)));
            return this;
        }

        public EasyApp<TOptions> AddProcessHandler(Func<TOptions, int> handler)
        {
            Handlers.Add(typeof(TOptions), new ProcessorHandler<TOptions>((result, console) => handler(result.Options)));
            return this;
        }

        public EasyApp<TOptions> AddProcessHandler(Func<TOptions, IEasyAppConsole, int> handler)
        {
            Handlers.Add(typeof(TOptions), new ProcessorHandler<TOptions>((result, console) => handler(result.Options, console)));
            return this;
        }

        public EasyApp<TOptions> AddCommandHandler<T>(Func<TOptions, T, int> handler)
        {
            Handlers.Add(typeof(T), new ProcessorHandler<TOptions>((result, console) => handler(result.Options, (T)console)));
            return this;
        }

        #endregion

        public int Run(string[] args)
        {
            try
            {
                var members = Reflector
                    .CollectMembers<TOptions>();

                var result = ParserBuilder
                    .Build<TOptions>(this, members)
                    .Parse(args);

                return ProcessorBuilder
                    .Build<TOptions>(this, members, result.Options)
                    .Process(result);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unhandled exception: {e}");
                return UnhandledExceptionExitCode;
            }
        }
    }
}
