using System.ComponentModel;

namespace EasyApp.parser.components
{
    public interface IValueConverter
    {
        object? Convert(Member member, string value);
    }

    public sealed class ValueConverter : IValueConverter
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
