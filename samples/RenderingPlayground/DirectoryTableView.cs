using System;
using System.CommandLine.Rendering;
using System.IO;
using System.Linq;
using static System.CommandLine.Rendering.Ansi;

namespace RenderingPlayground
{
    internal class DirectoryTableView : ConsoleView<DirectoryInfo>
    {
        public DirectoryTableView(ConsoleRenderer renderer, Region region = null) : base(renderer, region)
        {
            renderer.Formatter
                    .AddFormatter<DateTime>(d => $"{d:d} {Color.Foreground.DarkGray}{d:t}{Color.Foreground.Default}");
        }

        public override void Render(DirectoryInfo directory)
        {
            WriteLine();
            WriteLine();

            Write($"Directory: {directory.FullName}");

            WriteLine();
            WriteLine();

            var directoryContents = directory.EnumerateFileSystemInfos()
                                             .OrderBy(f => f is DirectoryInfo
                                                               ? 0
                                                               : 1);

            RenderTable(
                directoryContents,
                table => {
                    table.RenderColumn(
                        "Name".Underline(),
                        f => f is DirectoryInfo
                                 ? Span($"{Color.Foreground.LightGreen}{f.Name}{Color.Foreground.Default}")
                                 : Span($"{Color.Foreground.White}{f.Name}{Color.Foreground.Default}"));

                    table.RenderColumn(
                        "Created".Underline(),
                        f => Span(f.CreationTime));

                    table.RenderColumn(
                        "Modified".Underline(),
                        f => Span(f.LastWriteTime));
                });
        }
    }
}
