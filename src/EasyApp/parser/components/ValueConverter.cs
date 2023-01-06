using System.ComponentModel;

namespace EasyApp.parser.components
{
    internal interface IValueConverter
    {
        object? Convert(Member member, string value);
    }

    internal sealed class ValueConverter : IValueConverter
    {
        public object? Convert(Member member, string value)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(member.Type);
                return converter.ConvertFromInvariantString(value);
            }
            catch (Exception e)
            {
                throw new EasyAppException($"Failed to conert '{value}' as {member.Type.Name}.", e);
            }
        }
    }
}
