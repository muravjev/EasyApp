namespace EasyApp
{
    public interface IEasyAppHandler<TOptions>
    {
        int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console);
    }

    public sealed class EasyAppHandlers<TOptions> : Dictionary<Type, IEasyAppHandler<TOptions>> { }

    public sealed class EasyAppHandler<TOptions> : IEasyAppHandler<TOptions>
    {
        public readonly Func<EasyAppResult<TOptions>, IEasyAppConsole, int> Func;

        public EasyAppHandler(Func<EasyAppResult<TOptions>, IEasyAppConsole, int> func)
        {
            Func = func;
        }

        public int Handle(EasyAppResult<TOptions> result, IEasyAppConsole console)
        {
            return Func(result, console);
        }
    }
}
