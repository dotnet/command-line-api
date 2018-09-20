using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class Process
    {
        public static async Task<int> ExecuteAsync(
            string command,
            string args,
            CancellationToken? cancellationToken = null)
        {
            args = args ?? "";

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            using (var process = StartProcess(
                command,
                args,
                stdOut: data => stdOut.AppendLine(data),
                stdErr: data => stdErr.AppendLine(data)))
            {
                return await process.CompleteAsync(cancellationToken);
            }
        }

        public static async Task<int> CompleteAsync(
            this Diagnostics.Process process,
            CancellationToken? cancellationToken = null) =>
            await Task.Run(() =>
            {
                process.WaitForExit();

                return Task.FromResult(process.ExitCode);
            }, cancellationToken ?? CancellationToken.None);

        public static Diagnostics.Process StartProcess(
            string command,
            string args,
            Action<string> stdOut = null,
            Action<string> stdErr = null,
            params (string key, string value)[] environmentVariables)
        {
            args = args ?? "";

            var process = new Diagnostics.Process
            {
                StartInfo =
                {
                    Arguments = args,
                    FileName = command,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                }
            };

            if (environmentVariables?.Length > 0)
            {
                foreach (var tuple in environmentVariables)
                {
                    process.StartInfo.Environment.Add(tuple.key, tuple.value);
                }
            }

            if (stdOut != null)
            {
                process.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        stdOut(eventArgs.Data);
                    }
                };
            }

            if (stdErr != null)
            {
                process.ErrorDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        stdErr(eventArgs.Data);
                    }
                };
            }

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }
    }
}
