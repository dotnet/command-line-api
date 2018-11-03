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

            var formatter = new SpanFormatter();
            formatter.AddFormatter<DateTime>(d => $"{d:d} {ForegroundColorSpan.DarkGray()}{d:t}");


            Add(new ContentView(""));
            Add(new ContentView(""));

            Add(new ContentView($"Directory: {directory.FullName}"));

            Add(new ContentView(""));
            Add(new ContentView(""));
            
            var directoryContents = directory.EnumerateFileSystemInfos()
                                             .OrderBy(f => f is DirectoryInfo
                                                               ? 0
                                                               : 1).ToList();

            var tableView = new TableView<FileSystemInfo>();
            tableView.Items = directoryContents;
            tableView.AddColumn(f => f is DirectoryInfo
                                 ? Span($"{ForegroundColorSpan.LightGreen()}{f.Name}")
                                 : Span($"{ForegroundColorSpan.White()}{f.Name}") , 
                                 new ContentView("Name".Underline()));
            
            tableView.AddColumn(f => formatter.Format(f.CreationTime), new ContentView("Created".Underline()));
            tableView.AddColumn(f => formatter.Format(f.LastWriteTime), new ContentView("Modified".Underline()));

            Add(tableView);

            Span Span(FormattableString formattableString)
            {
                return formatter.ParseToSpan(formattableString);
            }
        }
    }
}
