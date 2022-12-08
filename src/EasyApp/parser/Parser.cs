using EasyApp.parser.components;

namespace EasyApp.parser
{
    public interface IParser<TOptions>
    {
        EasyAppResult<TOptions> Parse(string[] args);
    }

    public sealed class ParserState<TOptions>
    {
        public readonly TOptions Result;

        public readonly Stack<string> Args;

        public bool ParseKeys = true;

        public int ParameterIndex = 0;

        public bool IsHelp;

        internal ParserState(string[] args)
        {
            Result = Activator.CreateInstance<TOptions>();
            Args = new Stack<string>(args);
            IsHelp = args.Length == 0;
        }
    }

    public sealed class Parser<TOptions> : IParser<TOptions>
    {
        private readonly IKeyParser<TOptions> KeyParser;

        private readonly IParameterParser<TOptions> ParameterParser;

        private readonly IMembersValidator<TOptions> MembersValidator;

        public Parser(IKeyParser<TOptions> keyParser, IParameterParser<TOptions> parameterParser, IMembersValidator<TOptions> membersValidator)
        {
            KeyParser = keyParser;
            ParameterParser = parameterParser;
            MembersValidator = membersValidator;
        }

        private void parse(ParserState<TOptions> state)
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

        public EasyAppResult<TOptions> Parse(string[] args)
        {
            var nonEmptyArgs = args.Where(x => !string.IsNullOrEmpty(x)).Reverse().ToArray();
            var state = new ParserState<TOptions>(nonEmptyArgs);

            try
            {
                parse(state);
            }
            catch (EasyAppException exception)
            {
                return new EasyAppResult<TOptions>(state.Result, exception);
            }
            catch (Exception exception)
            {
                return new EasyAppResult<TOptions>(state.Result, new EasyAppException("Unknown parse exception.", exception));
            }

            return new EasyAppResult<TOptions>(state.Result, state.IsHelp);
        }
    }
}
