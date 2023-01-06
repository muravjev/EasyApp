namespace EasyApp
{
    public sealed class HelpSectionAttribute : SectionAttribute
    {
        public HelpSectionAttribute()
            : base(1, "Help", true, true) { }
    }
}
