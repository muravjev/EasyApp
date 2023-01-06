namespace EasyApp
{
    public sealed class AppException : Exception
    {
        public AppException(string message, Exception? innerException = null)
            : base(message, innerException) { }
    }
}
