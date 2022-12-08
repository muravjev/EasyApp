namespace EasyApp.parser
{
    public interface IParameterParser<TOptions>
    {
        void Parse(EasyAppParserState<TOptions> state, string arg);
    }

    public sealed class ParameterParser<TOptions> : IParameterParser<TOptions>
    {
        private readonly IValueConverter ValueConverter;

        private readonly Member[] ParameterMembers;

        public ParameterParser(IValueConverter valueConverter, Member[] parameterMembers)
        {
            ValueConverter = valueConverter;
            ParameterMembers = parameterMembers;
        }

        public void Parse(EasyAppParserState<TOptions> state, string arg)
        {
            if (state.ParameterIndex >= ParameterMembers.Length)
            {
                throw new AppException($"Unexpected parameter '{arg}'");
            }

            var parameter = ParameterMembers[state.ParameterIndex++];

            parameter.SetValue(state.Result, ValueConverter.Convert(parameter, arg));
        }
    }
}
