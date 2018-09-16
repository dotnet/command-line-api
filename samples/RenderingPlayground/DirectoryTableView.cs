using System;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Spans;
using System.IO;
using System.Linq;

namespace RenderingPlayground
{
    internal class DirectoryTableView : ConsoleView<DirectoryInfo>
    {
        public DirectoryTableView(ConsoleRenderer renderer, Region region = null) : base(renderer, region)
        {
            renderer.Formatter
                    .AddFormatter<DateTime>(d => $"{d:d} {ForegroundColorSpan.DarkGray}{d:t}{ForegroundColorSpan.Reset}");
        }

        protected override void OnRender(DirectoryInfo directory)
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

            //RenderTable(
            //    directoryContents,
            //    table => {
            //        table.RenderColumn(
            //            "Name".Underline(),
            //            f => f is DirectoryInfo
            //                     ? Span($"{ForegroundColorSpan.LightGreen}{f.Name}{ForegroundColorSpan.Reset}")
            //                     : Span($"{ForegroundColorSpan.White}{f.Name}{ForegroundColorSpan.Reset}"));

            //        table.RenderColumn(
            //            "Created".Underline(),
            //            f => Span(f.CreationTime));

            //        table.RenderColumn(
            //            "Modified".Underline(),
            //            f => Span(f.LastWriteTime));
            //    });
        }
    }
}
