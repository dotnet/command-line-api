namespace System.CommandLine
{
    public interface IHelpBuilder
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        string Generate(CommandDefinition commandDefinition);
    }
}
