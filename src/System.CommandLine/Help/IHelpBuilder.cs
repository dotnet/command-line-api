namespace System.CommandLine
{
    public interface IHelpBuilder
    {
        /// <summary>
        /// Sets the configuration properties to the specified, or default, values
        /// </summary>
        /// <param name="columnGutter"></param>
        /// /// <param name="indentationSize"></param>
        /// <param name="maxWidth"></param>
        void Configure(int? columnGutter = null, int? indentationSize = null, int? maxWidth = null);

        /// <summary>
        /// Resets the configuration properties to their default values
        /// </summary>
        void ResetConfiguration();

        /// <summary>
        ///
        /// </summary>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        string Generate(CommandDefinition commandDefinition);
    }
}
