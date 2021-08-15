namespace System.CommandLine.CommandGenerator.Invocations
{
    internal enum ReturnPattern
    {
        None,
        InvocationContextExitCode,
        FunctionReturnValue,
        AwaitFunction,
        AwaitFunctionReturnValue
    }
}
