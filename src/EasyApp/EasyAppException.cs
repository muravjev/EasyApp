namespace EasyApp
{
    public sealed class EasyAppException : Exception
    {
        public EasyAppException(string message, Exception? innerException = null)
            : base(message, innerException) { }
    }
}
