using System.Reflection;

namespace EasyApp.processor.components
{
    public interface IAppInfoProvider
    {
        string Product { get; }
        string Version { get; }
        string Title { get; }
        string Description { get; }
        string Copyright { get; }
        bool IsRelease { get; }
    }

    public sealed class AppInfoProvider : IAppInfoProvider
    {
        private readonly string Product;
        private readonly string Version;
        private readonly string Title;
        private readonly string Description;
        private readonly string Copyright;
        private readonly bool IsRelease;

        string IAppInfoProvider.Product => Product;

        string IAppInfoProvider.Version => Version;

        string IAppInfoProvider.Title => Title;

        string IAppInfoProvider.Description => Description;

        string IAppInfoProvider.Copyright => Copyright;

        bool IAppInfoProvider.IsRelease => IsRelease;

        #region Extensions

        private static string get<T>(Assembly assembly, Func<T, string> action)
        {
            if (assembly.GetCustomAttributes(typeof(T), true) is T[] attributes && attributes.Length == 1)
            {
                return action(attributes[0]);
            }

            return "Unknown";
        }

        #endregion

        public AppInfoProvider()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            Product = get<AssemblyProductAttribute>(assembly, a => a.Product);
            Version = get<AssemblyFileVersionAttribute>(assembly, a => a.Version);

            Title = get<AssemblyTitleAttribute>(assembly, a => a.Title);
            Description = get<AssemblyDescriptionAttribute>(assembly, a => a.Description);
            Copyright = get<AssemblyCopyrightAttribute>(assembly, a => a.Copyright);

#if DEBUG
            IsRelease = false;
#else
            IsRelease = true;
#endif
        }
    }
}