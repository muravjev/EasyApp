namespace EasyApp
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IsHiddenAttribute : Attribute
    {
    }
}
