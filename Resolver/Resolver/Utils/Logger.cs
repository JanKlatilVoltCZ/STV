using System;

namespace Resolver.Utils
{
    public class Logger : IDisposable
    {
        private readonly string source;
        public Logger(string source) => this.source = source;

        public void Log(string message) => Console.WriteLine($"{source}: {message}");
        public void Dispose()
        {
            Console.WriteLine($"{source}: DONE");
            Console.WriteLine();
        }
    }
}
