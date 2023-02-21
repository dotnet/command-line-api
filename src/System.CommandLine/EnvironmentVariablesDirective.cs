using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
    /// </summary>
    public sealed class EnvironmentVariablesDirective : Directive
    {
        public EnvironmentVariablesDirective() : base("env")
        {
        }

        public override void OnParsed(DirectiveResult directiveResult)
        {
            if (string.IsNullOrEmpty(directiveResult.Value))
            {
                return;
            }

            string[] components = directiveResult.Value.Split(new[] { '=' }, count: 2);
            string variable = components.Length > 0 ? components[0].Trim() : string.Empty;
            if (string.IsNullOrEmpty(variable) || components.Length < 2)
            {
                return;
            }

            string value = components[1].Trim();
            Environment.SetEnvironmentVariable(variable, value);
        }
    }
}
