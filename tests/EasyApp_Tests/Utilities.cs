using EasyApp.parser;

namespace EasyApp
{
    public static class Utilities
    {
        public static EasyAppResult<T> Parse<T>(params string[] args) where T : new()
        {
            var members = Reflector
                .CollectMembers<T>();

            var result = ParserBuilder
                .Build<T>(new EasyApp<T>(), members)
                .Parse(args);

            return result;
        }
    }
}