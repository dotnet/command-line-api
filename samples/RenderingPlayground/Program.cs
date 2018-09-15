using System;
using System.CommandLine.Rendering;
using System.Diagnostics;
using System.IO;

namespace RenderingPlayground
{
    class Program
    {
        /// <summary>
        /// Demonstrates various rendering capabilities.
        /// </summary>
        /// <param name="sample">&lt;colors|dir&gt; Renders a specified sample</param>
        /// <param name="height">The height of the rendering area</param>
        /// <param name="width">The width of the rendering area</param>
        /// <param name="top">The top position of the render area</param>
        /// <param name="left">The left position of the render area</param>
        /// <param name="virtualTerminalMode">Enable virtual terminal mode</param>
        /// <param name="text">The text to render</param>
        /// <param name="outputMode">&lt;Ansi|NonAnsi|File&gt; Sets the output mode</param>
        /// <param name="overwrite">Overwrite the specified region. (If not, scroll.)</param>
        public static void Main(
            string sample = "",
            int? height = null,
            int? width = null,
            int top = 0,
            int left = 0,
            bool virtualTerminalMode = true,
            string text = null,
            OutputMode outputMode = OutputMode.Ansi,
            bool overwrite = true)
        {
            VirtualTerminalMode vt = null;

            try
            {
                if (outputMode == OutputMode.Auto &&
                    Console.IsOutputRedirected)
                {
                    overwrite = false;
                    outputMode = OutputMode.File;
                }

                var region = new Region(left,
                                        top,
                                        width ?? Console.WindowWidth,
                                        height ?? Console.WindowHeight,
                                        overwrite);

                var writer = new ConsoleRenderer(mode: outputMode);

                if (virtualTerminalMode &&
                    outputMode != OutputMode.File)
                {
                    vt = VirtualTerminalMode.TryEnable();

                    // TODO: (Main) implement this in the core
                    if (overwrite)
                    {
                        if (vt.IsEnabled)
                        {
                            writer.Console.Out.WriteLine(Ansi.Clear.EntireScreen);
                        }
                        else
                        {
                            Console.Clear();
                        }
                    }
                }

                switch (sample)
                {
                    case "colors":
                        new ColorsView(writer, region).Render(text ?? "*");
                        break;

                    case "dir":
                        new DirectoryTableView(writer, region)
                            .Render(new DirectoryInfo(Directory.GetCurrentDirectory()));
                        break;

                    case "moby":
                        writer.RenderToRegion(
                            $"Call me {StyleSpan.BoldOn}{StyleSpan.UnderlinedOn}Ishmael{StyleSpan.UnderlinedOff}{StyleSpan.BoldOff}. Some years ago -- never mind how long precisely -- having little or no money in my purse, and nothing particular to interest me on shore, I thought I would sail about a little and see the watery part of the world. It is a way I have of driving off the spleen and regulating the circulation. Whenever I find myself growing grim about the mouth; whenever it is a damp, drizzly November in my soul; whenever I find myself involuntarily pausing before coffin warehouses, and bringing up the rear of every funeral I meet; and especially whenever my hypos get such an upper hand of me, that it requires a strong moral principle to prevent me from deliberately stepping into the street, and {ForegroundColorSpan.Rgb(60, 0, 0)}methodically{ForegroundColorSpan.Reset} {ForegroundColorSpan.Rgb(90, 0, 0)}knocking{ForegroundColorSpan.Reset} {ForegroundColorSpan.Rgb(120, 0, 0)}people's{ForegroundColorSpan.Reset} {ForegroundColorSpan.Rgb(160, 0, 0)}hats{ForegroundColorSpan.Reset} {ForegroundColorSpan.Rgb(220, 0, 0)}off{ForegroundColorSpan.Reset} then, I account it high time to get to sea as soon as I can. This is my substitute for pistol and ball. With a philosophical flourish Cato throws himself upon his sword; I quietly take to the ship. There is nothing surprising in this. If they but knew it, almost all men in their degree, some time or other, cherish very nearly the same feelings towards the ocean with me.",
                            region);
                        break;

                    case "processes":
                        new ProcessesView(writer, region)
                            .Render(Process.GetProcesses());
                        break;

                    default:
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            writer.RenderToRegion(
                                text,
                                region);
                        }
                        else
                        {
                            writer.RenderToRegion(
                                $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset} fox jumps over the lazy dog.",
                                region);
                        }

                        break;
                }

                if (!Console.IsOutputRedirected)
                {
                    Console.ReadKey();
                }
            }
            finally
            {
                vt?.Dispose();
            }
        }
    }
}
