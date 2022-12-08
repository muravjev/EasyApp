using EasyApp.parser;

namespace EasyApp
{
    public sealed class EasyAppParserResult<TOptions>
    {
        // Parse options.
        public readonly TOptions Options;

        // Whether execution is breaked and help is required (no args or help flag like --version).
        public readonly bool IsHelp = false;

        // Whether there were an exception during parsing.
        public readonly Exception? Exception = null;

        // Whether parsing is succeesfull.
        public bool IsParsed => Exception == null;

        public EasyAppParserResult(TOptions options, bool isHelp)
        {
            Options = options;
            IsHelp = isHelp;
        }

        public EasyAppParserResult(TOptions options, Exception exception)
        {
            Options = options;
            Exception = exception;
        }
    }

    public interface IEasyAppParser<TOptions>
    {
        EasyAppParserResult<TOptions> Parse(string[] args);
    }

    public sealed class EasyAppParserState<TOptions>
    {
        public readonly TOptions Result;

        public readonly Stack<string> Args;

        public bool ParseKeys = true;

        public int ParameterIndex = 0;

        public bool IsHelp;

        internal EasyAppParserState(string[] args)
        {
            Result = Activator.CreateInstance<TOptions>();
            Args = new Stack<string>(args);
            IsHelp = args.Length == 0;
        }
    }

    public sealed class EasyAppParser<TOptions> : IEasyAppParser<TOptions>
    {
        private readonly IKeyParser<TOptions> KeyParser;

        private readonly IParameterParser<TOptions> ParameterParser;

        private readonly IMembersValidator<TOptions> MembersValidator;

        public EasyAppParser(IKeyParser<TOptions> keyParser, IParameterParser<TOptions> parameterParser, IMembersValidator<TOptions> membersValidator)
        {
            KeyParser = keyParser;
            ParameterParser = parameterParser;
            MembersValidator = membersValidator;
        }

        private void parse(EasyAppParserState<TOptions> state)
        {
            while (state.Args.Count > 0 && state.IsHelp == false)
            {
                var arg = state.Args.Pop();

                if (state.ParseKeys)
                {
                    if (arg == "--")
                    {
                        state.ParseKeys = false;
                        continue;
                    }

                    if (KeyParser.Parse(state, arg))
                    {
                        continue;
                    }
                }

                ParameterParser.Parse(state, arg);
            }

            if (!state.IsHelp)
            {
                MembersValidator.Validate(state.Result);
            }
        }

        public EasyAppParserResult<TOptions> Parse(string[] args)
        {
            var nonEmptyArgs = args.Where(x => !string.IsNullOrEmpty(x)).Reverse().ToArray();
            var state = new EasyAppParserState<TOptions>(nonEmptyArgs);

            try
            {
                parse(state);
            }
            catch (AppException exception)
            {
                return new EasyAppParserResult<TOptions>(state.Result, exception);
            }
            catch (Exception exception)
            {
                return new EasyAppParserResult<TOptions>(state.Result, new AppException("Unknown parse exception.", exception));
            }

            return new EasyAppParserResult<TOptions>(state.Result, state.IsHelp);
        }
    }
}
