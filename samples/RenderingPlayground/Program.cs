using System;
using System.CommandLine.Rendering;
using System.Threading.Tasks;
using static System.CommandLine.Rendering.Ansi;

namespace RenderingPlayground
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            ConsoleWriter writer;

            // non-ANSI output, non-ANSI  terminal
            writer = new ConsoleWriter(mode: OutputMode.NonAnsi);

            writer.RenderToRegion(
                $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                new Region(4, 4, 0, 0));


            // ANSI output, non-ANSI terminal
            writer = new ConsoleWriter(mode: OutputMode.Ansi);

            writer.RenderToRegion(
                $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                new Region(4, 4, 8, 8));

            using (VirtualTerminalMode.TryEnable())
            {

                // ANSI output, ANSI terminal
                writer = new ConsoleWriter(mode: OutputMode.Ansi);

                writer.RenderToRegion(
                    $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                    new Region(4, 4, 16, 16));

                // ... and again
                writer.RenderToRegion(
                    $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                    new Region(10, 10, 25, 25));
            }

            Console.ReadKey();
        }
    }
}
