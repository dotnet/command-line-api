namespace System.CommandLine
{
    internal static class Platform
    {
        private static bool? _isConsoleRedirectionCheckSupported;

        public static bool IsConsoleRedirectionCheckSupported
        {
            get
            {
                if (_isConsoleRedirectionCheckSupported is null)
                {
                    try
                    {
                        var check = Console.IsOutputRedirected;
                        _isConsoleRedirectionCheckSupported = true;
                    }

                    catch (PlatformNotSupportedException)
                    {
                        _isConsoleRedirectionCheckSupported = false;
                    }
                }

                return _isConsoleRedirectionCheckSupported.Value;
            }
        }
    }
}
