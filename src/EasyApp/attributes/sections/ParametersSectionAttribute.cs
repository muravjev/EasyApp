namespace EasyApp
{
    public sealed class ParametersSectionAttribute : SectionAttribute
    {
        public ParametersSectionAttribute()
            : base(4, "Parameters", true, false) { }
    }
}
