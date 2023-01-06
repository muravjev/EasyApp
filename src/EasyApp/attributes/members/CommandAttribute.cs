namespace EasyApp
{
    public class CommandAttribute : MemberAttribute
    {
        private static readonly SectionAttribute defaultSection = new CommandsSectionAttribute();

        public override MemberType Type => MemberType.Command;

        public override SectionAttribute DefaultSection => throw new NotImplementedException();

        public CommandAttribute(string name, string description)
            : base(name, null, null, description, false) { }
    }
}
