using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EasyApp.parser
{
    public record Arg(string key, string name, string value);

    public interface IKeyArgParser
    {
        Arg? Parse(string arg);
    }

    public sealed class KeyArgParser : IKeyArgParser
    {
        public Arg? Parse(string arg)
        {
            var regex = new Regex(@"(--|-|\/)([^:=]+)[:=]?(.*)?", RegexOptions.Compiled);
            var matches = regex.Matches(arg);
            if (matches.Count == 0)
            {
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    throw new EasyAppException($"Invalid Key '{arg}'.");
                }

                return null;
            }

            Debug.Assert(matches.Count == 1);

            var values = matches[0].Groups.Values.Skip(1).Select(x => x.Value).ToArray();
            Debug.Assert(values.Length == 3);

            var key = values[0];
            var name = values[1];
            var value = values[2];

            return new Arg(key, name, value);
        }
    }
}
