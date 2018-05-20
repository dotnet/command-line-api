﻿using System.IO;

namespace System.CommandLine.DragonFruit
{
    /// <summary>
    /// An implementation of <see cref="IConsole"/> that wraps <see cref="System.Console"/>.
    /// </summary>
    internal class PhysicalConsole : IConsole
    {
        private PhysicalConsole()
        { }

        public static IConsole Instance { get; } = new PhysicalConsole();

        public TextWriter Error => Console.Error;

        public TextWriter Out => Console.Out;

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public void ResetColor() => Console.ResetColor();
    }
}
