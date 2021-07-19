using System;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.IO;
using System.Linq;

namespace RenderingPlayground
{
    internal class DirectoryTableView : StackLayoutView
    {
        public DirectoryTableView(DirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            Add(new ContentView("\n"));
            Add(new ContentView(Span($"Directory: {directory.FullName.Rgb(235, 30, 180)}")));
            Add(new ContentView("\n"));

            var tableView = new TableView<FileSystemInfo>();

            tableView.Items = directory.EnumerateFileSystemInfos()
                                       .OrderByDescending(f => f is DirectoryInfo)
                                       .ToList();

            tableView.AddColumn(
                cellValue: f => f is DirectoryInfo
                                    ? f.Name.LightGreen()
                                    : f.Name.White(),
                header: new ContentView("Name".Underline()));

            tableView.AddColumn(
                cellValue: f => Span(f.CreationTime),
                header: new ContentView("Created".Underline()));

            tableView.AddColumn(
                cellValue: f => Span(f.LastWriteTime),
                header: new ContentView("Modified".Underline()));

            Add(tableView);

            Formatter.AddFormatter<DateTime>(d => $"{d:d} {ForegroundColorSpan.DarkGray()}{d:t}{ForegroundColorSpan.Reset()}");
        }

        TextSpan Span(FormattableString formattableString)
        {
            return Formatter.ParseToSpan(formattableString);
        }

        TextSpan Span(object obj)
        {
            return Formatter.Format(obj);
        }

        protected TextSpanFormatter Formatter { get; } = new TextSpanFormatter();
    }
}
