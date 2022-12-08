using EasyApp.parser.components;

namespace EasyApp.parser
{
    internal interface IParserBuilder<TOptions>
    {
        IParser<TOptions> Build();
    }

    internal sealed class ParserBuilder<TOptions> : IParserBuilder<TOptions>
    {
        public IParser<TOptions> Build()
        {
            var members = Reflector.CollectMembers<TOptions>();

            var shortKeyMembers = members.GroupByKey(x => x.Attribute.ShortKey);
            var longKeyMembers = members.GroupByKey(x => x.Attribute.LongKey);
            var parameterMembers = members.Filter(MemberType.Parameter);

            var valueConverter = new ValueConverter();

            return new Parser<TOptions>(
                new KeyParser<TOptions>(
                    new KeyArgParser(),
                    new KeyMemberLocator(shortKeyMembers, longKeyMembers),
                    valueConverter),
                new ParameterParser<TOptions>(valueConverter, parameterMembers),
                new MembersValidator<TOptions>(members)
            );
        }
    }
}
