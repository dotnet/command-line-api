namespace System.CommandLine
{
    public interface IHelpBuilder
    {
        /// <summary>
        /// Writes help text for the provided <see cref="CommandDefinition"/> to
        /// the configured <see cref="IConsole"/> instance
        /// </summary>
        /// <param name="commandDefinition">
        /// The <see cref="CommandDefinition"/> to generate and write text for
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        void Write(CommandDefinition commandDefinition);
    }
}
