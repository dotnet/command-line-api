namespace System.CommandLine.Suggest
{
    public class Registration
    {
        public Registration(string executablePath)
        {
            ExecutablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
        }

        public string ExecutablePath { get; }
    }
}
