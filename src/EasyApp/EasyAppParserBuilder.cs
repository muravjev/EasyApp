using EasyApp.parser;

namespace EasyApp
{
    public interface IEasyAppParserBuilder<TOptions>
    {
        IEasyAppParser<TOptions> Build();
    }

    public sealed class EasyAppParserBuilder<TOptions> : IEasyAppParserBuilder<TOptions>
    {
        public IEasyAppParser<TOptions> Build()
        {
            var members = Reflector.CollectMembers<TOptions>();

            var shortKeyMembers = members.GroupByKey(x => x.Attribute.ShortKey);
            var longKeyMembers = members.GroupByKey(x => x.Attribute.LongKey);
            var parameterMembers = members.Filter(MemberType.Parameter);

            var valueConverter = new ValueConverter();

            return new EasyAppParser<TOptions>(
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
