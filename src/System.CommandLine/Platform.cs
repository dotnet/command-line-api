namespace System.CommandLine
{
    internal static class Platform
    {
        private static bool? _isConsoleRedirectionCheckSupported;
        public static bool IsConsoleRedirectionCheckSupported
        {
            get
            {
                if (_isConsoleRedirectionCheckSupported == null)
                {
                    try
                    {
                        var check = Console.IsOutputRedirected;
                        _isConsoleRedirectionCheckSupported = false;
                    }

                    catch (PlatformNotSupportedException)
                    {
                        _isConsoleRedirectionCheckSupported = true;
                    }
                }

                return _isConsoleRedirectionCheckSupported.Value;
            }
        }
    }
}
