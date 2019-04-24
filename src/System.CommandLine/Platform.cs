namespace System.CommandLine
{
    internal static class Platform
    {
        private static bool? _isWasm;
        public static bool IsWasm
        {
            get
            {
                if (_isWasm == null)
                {
                    try
                    {
                        var check = Console.IsOutputRedirected;
                        _isWasm = false;
                    }

                    catch (PlatformNotSupportedException)
                    {
                        _isWasm = true;
                    }
                }

                return _isWasm.Value;
            }
        }
    }
}
