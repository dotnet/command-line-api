namespace System.CommandLine.CommandHandler.Invocations
{
    public enum ReturnPattern
    {
        None,
        InvocationContextExitCode,
        FunctionReturnValue,
        AwaitFunction,
        AwaitFunctionReturnValue
    }
}
