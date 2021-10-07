namespace System.CommandLine.Generator.Invocations
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
