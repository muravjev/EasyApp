using EasyApp.parser;

namespace EasyApp
{
    public static class Utilities
    {
        public static EasyAppResult<T> Parse<T>(params string[] args) where T : new()
        {
            return new ParserBuilder<T>().Build().Parse(args);
        }
    }
}