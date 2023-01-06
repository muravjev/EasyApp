namespace EasyApp.parser.components
{
    internal interface IParameterParser<TOptions>
    {
        void Parse(ParserState<TOptions> state, string arg);
    }

    internal sealed class ParameterParser<TOptions> : IParameterParser<TOptions>
    {
        private readonly IValueConverter ValueConverter;

        private readonly Member[] ParameterMembers;

        public ParameterParser(IValueConverter valueConverter, Member[] parameterMembers)
        {
            ValueConverter = valueConverter;
            ParameterMembers = parameterMembers;
        }

        public void Parse(ParserState<TOptions> state, string arg)
        {
            if (state.ParameterIndex >= ParameterMembers.Length)
            {
                throw new EasyAppException($"Unexpected parameter '{arg}'");
            }

            var parameter = ParameterMembers[state.ParameterIndex++];

            parameter.SetValue(state.Result, ValueConverter.Convert(parameter, arg));
        }
    }
}
