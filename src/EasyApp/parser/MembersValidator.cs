namespace EasyApp.parser
{
    public interface IMembersValidator<TOptions>
    {
        void Validate(TOptions options);
    }

    public sealed class MembersValidator<TOptions> : IMembersValidator<TOptions>
    {
        private readonly Member[] Members;

        public MembersValidator(Member[] members)
        {
            Members = members;
        }

        public void Validate(TOptions options)
        {
            foreach (var member in Members)
            {
                if (member.Attribute.IsRequired && member.GetValue(options) == null)
                {
                    throw new EasyAppException($"Value for {member.Attribute.Type.ToString().ToLower()} '{member.Attribute.Name}' is required.");
                }
            }
        }
    }
}
