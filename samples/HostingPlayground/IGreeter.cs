using System;

namespace HostingPlayground
{
    interface IGreeter
    {
        void Greet(string name) => Console.WriteLine($"Hello, {name ?? "anonymous"}");
    }
}
