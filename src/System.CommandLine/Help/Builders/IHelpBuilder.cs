namespace System.CommandLine
{
    public interface IHelpBuilder
    {
        /// <summary>
        /// Writes help text for the provided <see cref="ICommand"/> to
        /// the configured <see cref="IConsole"/> instance
        /// </summary>
        /// <param name="command">
        /// The <see cref="ICommand"/> to generate and write text for
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        void Write(ICommand command);
    }
}
