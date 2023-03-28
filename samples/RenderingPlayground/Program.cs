// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Process = System.Diagnostics.Process;

namespace RenderingPlayground
{
    class Program
    {
        /// <summary>
        /// Demonstrates various rendering capabilities.
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="sample">Renders a specified sample</param>
        /// <param name="height">The height of the rendering area</param>
        /// <param name="width">The width of the rendering area</param>
        /// <param name="top">The top position of the render area</param>
        /// <param name="left">The left position of the render area</param>
        /// <param name="text">The text to render</param>
        /// <param name="overwrite">Overwrite the specified region. (If not, scroll.)</param>
        public static void Main(
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
            ParseResult parseResult,
            SampleName sample = SampleName.Dir,
            int? height = null,
            int? width = null,
            int top = 0,
            int left = 0,
            string text = null,
            bool overwrite = true)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        {
            // Should this have a concrete reference to Console?
            var region = new Region(left,
                                    top,
                                    width ?? Console.WindowWidth,
                                    height ?? Console.WindowHeight,
                                    overwrite);
            
            var console = new SystemConsole();

            if (overwrite && 
                console is ITerminal terminal)
            {
                terminal.Clear();
            }

            var consoleRenderer = new ConsoleRenderer(
                console,
                mode: OutputMode.Auto,
                resetAfterRender: true);

            switch (sample)
            {
                case SampleName.Colors:
                {
                    var screen = new ScreenView(renderer: consoleRenderer, console);
                    screen.Child = new ColorsView(text ?? "*");

                    screen.Render(region);
                }
                    break;

                case SampleName.Dir:
                    
                    var directoryTableView = new DirectoryTableView(
                        new DirectoryInfo(Directory.GetCurrentDirectory()));

                    console.Append(directoryTableView);

                    break;

                case SampleName.Moby:
                    consoleRenderer.RenderToRegion(
                        $"Call me {StyleSpan.BoldOn()}{StyleSpan.UnderlinedOn()}Ishmael{StyleSpan.UnderlinedOff()}{StyleSpan.BoldOff()}. Some years ago -- never mind how long precisely -- having little or no money in my purse, and nothing particular to interest me on shore, I thought I would sail about a little and see the watery part of the world. It is a way I have of driving off the spleen and regulating the circulation. Whenever I find myself growing grim about the mouth; whenever it is a damp, drizzly November in my soul; whenever I find myself involuntarily pausing before coffin warehouses, and bringing up the rear of every funeral I meet; and especially whenever my hypos get such an upper hand of me, that it requires a strong moral principle to prevent me from deliberately stepping into the street, and {ForegroundColorSpan.Rgb(60, 0, 0)}methodically{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(90, 0, 0)}knocking{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(120, 0, 0)}people's{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(160, 0, 0)}hats{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(220, 0, 0)}off{ForegroundColorSpan.Reset()} then, I account it high time to get to sea as soon as I can. This is my substitute for pistol and ball. With a philosophical flourish Cato throws himself upon his sword; I quietly take to the ship. There is nothing surprising in this. If they but knew it, almost all men in their degree, some time or other, cherish very nearly the same feelings towards the ocean with me.",
                        region);
                    break;

                case SampleName.Processes:
                {
                    var view = new ProcessesView(Process.GetProcesses());
                    view.Render(consoleRenderer, region);
                }

                    break;

                case SampleName.TableView:
                {
                    var table = new TableView<Process>
                                {
                                    Items = Process.GetProcesses().Where(x => !string.IsNullOrEmpty(x.MainWindowTitle)).OrderBy(p => p.ProcessName).ToList()
                                };
                    table.AddColumn(process => $"{process.ProcessName} ", "Name");
                    table.AddColumn(process => ContentView.FromObservable(process.TrackCpuUsage(), x => $"{x.UsageTotal:P}"), "CPU", ColumnDefinition.Star(1));

                    var screen = new ScreenView(renderer: consoleRenderer, console) { Child = table };
                    screen.Render();
                }
                    break;

                case SampleName.Clock:
                {
                    var screen = new ScreenView(renderer: consoleRenderer, console);
                    var lastTime = DateTime.Now;
                    var clockObservable = new BehaviorSubject<DateTime>(lastTime);
                    var clockView = ContentView.FromObservable(clockObservable, x => $"{x:T}");
                    screen.Child = clockView;
                    screen.Render();

                    while (!Console.KeyAvailable)
                    {
                        if (DateTime.Now - lastTime > TimeSpan.FromSeconds(1))
                        {
                            lastTime = DateTime.Now;
                            clockObservable.OnNext(lastTime);
                        }
                    }
                }
                    break;

                case SampleName.GridLayout:
                {
                    var screen = new ScreenView(renderer: consoleRenderer, console);
                    var content = new ContentView(
                        "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum for Kevin.");
                    var smallContent = new ContentView("Kevin Bost");
                    var longContent = new ContentView("Hacking on System.CommandLine");

                    var gridView = new GridView();
                    gridView.SetColumns(
                        ColumnDefinition.SizeToContent(),
                        ColumnDefinition.Star(1),
                        ColumnDefinition.Star(0.5)
                    );
                    gridView.SetRows(
                        RowDefinition.Star(0.5),
                        RowDefinition.Star(0.5)
                    );

                    gridView.SetChild(smallContent, 0, 0);
                    gridView.SetChild(longContent, 0, 1);
                    //gridView.SetChild(content, 0, 0);
                    gridView.SetChild(content, 1, 1);
                    gridView.SetChild(content, 2, 0);

                    screen.Child = gridView;

                    screen.Render();
                }
                    break;

                case SampleName.Cursor:
                    {
                        var gridView = new GridView();
                        gridView.SetColumns(ColumnDefinition.SizeToContent());
                        gridView.SetRows(
                            RowDefinition.SizeToContent(),
                            RowDefinition.Star(1)
                        );
                        var content = new ContentView("Instructions:\n" +
                            $"DIRECTION ARROWS move the cursor; CTRL moves 2 instead of 1.\n" +
                            "PAGE UP/DOWN scrolls up/down.\n" +
                            "S saves the cursor position, R restores it.\n" +
                            "ENTER navigates to the start of the next line; CTRL moves 2 instead of 1.\n" +
                            "L moves to location (3, 9).\n" +
                            "ESC quits.");
                        gridView.SetChild(content, 0, 0);
                        gridView.SetChild(new ColorsView("#"), 0, 1);

                        var screen = new ScreenView(renderer: consoleRenderer, console)
                        {
                            Child = gridView
                        };
                        screen.Render(region);

                        // move the cursor to the home position.
                        console.Out.Write($"{Ansi.Cursor.Move.ToUpperLeftCorner}");
                        console.Out.Write($"{Ansi.Cursor.Show}");

                        // input seems not to be supported by the interfaces; how can this be got without using Console?
                        var key = Console.ReadKey(true);

                        // This appears to be necessary to get the application to listen for *any* modifier key.
                        Console.TreatControlCAsInput = true;
                        while (key.Key != ConsoleKey.Escape)
                        {
                            var lines = !key.Modifiers.HasFlag(ConsoleModifiers.Control) ? default : 2;
                            switch (key.Key)
                            {
                                case ConsoleKey.DownArrow:
                                    console.Out.Write($"{Ansi.Cursor.Move.Down(lines)}");
                                    break;

                                case ConsoleKey.UpArrow:
                                    console.Out.Write($"{Ansi.Cursor.Move.Up(lines)}");
                                    break;

                                case ConsoleKey.RightArrow:
                                    console.Out.Write($"{Ansi.Cursor.Move.Right(lines)}");
                                    break;

                                case ConsoleKey.LeftArrow:
                                    console.Out.Write($"{Ansi.Cursor.Move.Left(lines)}");
                                    break;

                                case ConsoleKey.PageUp:
                                    console.Out.Write($"{Ansi.Cursor.Scroll.DownOne}");
                                    break;

                                case ConsoleKey.PageDown:
                                    console.Out.Write($"{Ansi.Cursor.Scroll.UpOne}");
                                    break;

                                case ConsoleKey.Enter:
                                    console.Out.Write($"{Ansi.Cursor.Move.NextLine(lines)}");
                                    break;

                                case ConsoleKey.S:
                                    console.Out.Write($"{Ansi.Cursor.SavePosition}");
                                    break;

                                case ConsoleKey.R:
                                    console.Out.Write($"{Ansi.Cursor.RestorePosition}");
                                    break;

                                case ConsoleKey.L:
                                    console.Out.Write($"{Ansi.Cursor.Move.ToLocation(3, 9)}");
                                    break;

                                case ConsoleKey.C:
                                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                                    {
                                        // mimic the standard CTRL+C behaviour.
                                        Environment.Exit(1);
                                    }

                                    break;
                            }

                            key = Console.ReadKey(true);
                        }
                    }

                    // reset the screen and cursor.
                    console.GetTerminal().Clear();
                    console.Out.Write($"{Ansi.Cursor.Move.ToUpperLeftCorner}");

                    return;

                default:
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        consoleRenderer.RenderToRegion(
                            text,
                            region);
                    }
                    else
                    {
                        var screen = new ScreenView(renderer: consoleRenderer, console);
                        var stackLayout = new StackLayoutView();
                        var content1 = new ContentView("Hello World!");
                        var content2 = new ContentView(
                            "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum for Kevin.");
                        stackLayout.Add(content2);
                        stackLayout.Add(content1);
                        stackLayout.Add(content2);
                        screen.Child = stackLayout;
                        screen.Render(new Region(0, 0, 50, Size.MaxValue));
                        //screen.Render(writer);
                    }

                    break;
            }

            if (!Console.IsOutputRedirected)
            {
                Console.ReadKey();
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
