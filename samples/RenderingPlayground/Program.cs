using System;
using System.CommandLine.Rendering;
using System.IO;
using System.Linq;
using static System.CommandLine.Rendering.Ansi;

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
        public static void Main(
            string sample = "",
            int? height = null,
            int? width = null,
            int top = 0,
            int left = 0,
            bool virtualTerminalMode = true,
            string text = null,
            OutputMode outputMode = OutputMode.Ansi)
        {
            var region = new Region(left,
                                    top,
                                    width ?? Console.WindowWidth,
                                    height ?? Console.WindowHeight, true);

            VirtualTerminalMode vt = null;

            try
            {
                var writer = new ConsoleRenderer(mode: outputMode);

                if (virtualTerminalMode)
                {
                    vt = VirtualTerminalMode.TryEnable();

                    // TODO: (Main) implement this in the core
                    if (vt.IsEnabled)
                    {
                        writer.Console.Out.WriteLine(Clear.EntireScreen);
                    }
                    else
                    {
                        Console.Clear();
                    }
                }

                switch (sample)
                {
                    case "colors":
                        new ColorsView(writer, region).Render(text);
                        break;

                    case "dir":
                        new DirectoryTableView(writer, region)
                            .Render(new DirectoryInfo(Directory.GetCurrentDirectory()));
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
                                $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                                region);
                        }

                        break;
                }

                Console.ReadKey();
            }
            finally
            {
                vt?.Dispose();
            }
        }
    }
}
