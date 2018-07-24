using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    public class ConsoleTable<T>
    {
        public ConsoleWriter ConsoleWriter { get; }

        public ConsoleTable(ConsoleWriter consoleWriter)
        {
            ConsoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
        }

        public void RenderColumn(
            string header,
            Func<T, object> cell)
        {
            Columns.Add(new ConsoleTableColumn<T>($"{header}", cell, ConsoleWriter));
        }

        internal IList<ConsoleTableColumn<T>> Columns { get; } = new List<ConsoleTableColumn<T>>();
    }
}
