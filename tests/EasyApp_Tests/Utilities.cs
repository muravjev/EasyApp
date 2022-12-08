namespace EasyApp
{
    public static class Utilities
    {
        public static EasyAppResult<T> Parse<T>(params string[] args) where T : new()
        {
            return new EasyAppParserBuilder<T>().Build().Parse(args);
        }
    }
}