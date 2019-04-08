namespace System.CommandLine.Benchmarks.Helpers
{
    /// <summary>
    /// Wraps instance of type <typeparamref name="T"/> and
    /// returns a separately provided comment to the instance when DotnetBenchmark logs results.
    /// </summary>
    /// <remarks>
    /// The purpose of this wrapper is to increase the readability of DotnetBenchmark outputs.
    /// </remarks>
    public class BdnParam<T>
    {
        public T Value { get; }
        private readonly string _comment;

        public BdnParam(T value, string comment)
        {
            Value = value;
            _comment = comment;
        }

        public override string ToString()
        {
            return _comment;
        }
    }
}
