namespace EasyApp
{
    public interface IEasyAppHandler<TOptions>
    {
        int Handle(EasyAppParserResult<TOptions> result, IEasyAppConsole console);
    }

    public sealed class EasyAppHandlers<TOptions> : Dictionary<Type, IEasyAppHandler<TOptions>> { }

    public sealed class EasyAppHandler<TOptions> : IEasyAppHandler<TOptions>
    {
        public readonly Func<EasyAppParserResult<TOptions>, IEasyAppConsole, int> Func;

        public EasyAppHandler(Func<EasyAppParserResult<TOptions>, IEasyAppConsole, int> func)
        {
            Func = func;
        }

        public int Handle(EasyAppParserResult<TOptions> result, IEasyAppConsole console)
        {
            return Func(result, console);
        }
    }
}
