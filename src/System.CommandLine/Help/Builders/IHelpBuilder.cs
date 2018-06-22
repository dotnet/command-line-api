namespace System.CommandLine
{
    public interface IHelpBuilder
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="commandDefinition"></param>
        /// <exception cref="ArgumentNullException"></exception>
        void Generate(CommandDefinition commandDefinition);
    }
}
