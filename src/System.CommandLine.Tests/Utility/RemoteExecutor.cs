// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Tests.Utility
{
    public class RemoteExecutor
    {
        public static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("This is not the program you are looking for. Run 'dotnet test' instead.");
                return -1;
            }

            string typeName = args[0];
            string methodName = args[1];
            string exceptionFile = args[2];
            string[] methodArguments = args.Skip(3).ToArray();

            Type type = null;
            MethodInfo methodInfo = null;
            object instance = null;
            int exitCode = 0;
            try
            {
                type = typeof(RemoteExecutor).Assembly.GetType(typeName);
                methodInfo = type.GetTypeInfo().GetDeclaredMethod(methodName);
                instance = null;

                if (!methodInfo.IsStatic)
                {
                    instance = Activator.CreateInstance(type);
                }

                object result = methodInfo.Invoke(instance, new object[] { methodArguments });
                if (result is Task<int> task)
                {
                    exitCode = task.GetAwaiter().GetResult();
                }
                else if (result is int exit)
                {
                    exitCode = exit;
                }
            }
            catch (Exception exc)
            {
                if (exc is TargetInvocationException && exc.InnerException != null)
                    exc = exc.InnerException;

                var output = new StringBuilder();
                output.AppendLine();
                output.AppendLine("Child exception:");
                output.AppendLine("  " + exc);
                output.AppendLine();
                output.AppendLine("Child process:");
                output.AppendLine($"  {type} {methodInfo}");
                output.AppendLine();

                if (methodArguments.Length > 0)
                {
                    output.AppendLine("Child arguments:");
                    output.AppendLine("  " + string.Join(", ", methodArguments));
                }

                File.WriteAllText(exceptionFile, output.ToString());
            }
            finally
            {
                (instance as IDisposable)?.Dispose();
            }

            return exitCode;
        }

        public static RemoteExecution Execute(Func<string[], int> mainMethod, string[] args = null, ProcessStartInfo psi = null)
            => Execute(mainMethod.GetMethodInfo(), args, psi);

        public static RemoteExecution Execute(Func<string[], Task<int>> mainMethod, string[] args = null, ProcessStartInfo psi = null)
            => Execute(mainMethod.GetMethodInfo(), args, psi);

        private static RemoteExecution Execute(MethodInfo methodInfo, string[] args, ProcessStartInfo psi)
        {
            Type declaringType = methodInfo.DeclaringType;
            string className = declaringType.FullName;
            string methodName = methodInfo.Name;
            string exceptionFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            string dotnetExecutable = Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string thisAssembly = typeof(RemoteExecutor).Assembly.Location;
            var assembly = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
            string entryAssemblyWithoutExtension = Path.Combine(Path.GetDirectoryName(assembly.Location),
                                                                Path.GetFileNameWithoutExtension(assembly.Location));
            string runtimeConfig = GetApplicationArgument("--runtimeconfig");
            if (runtimeConfig == null)
            {
                runtimeConfig = entryAssemblyWithoutExtension + ".runtimeconfig.json";
            }
            string depsFile = GetApplicationArgument("--depsfile");
            if (depsFile == null)
            {
                depsFile = entryAssemblyWithoutExtension + ".deps.json";
            }

            if (psi == null)
            {
                psi = new ProcessStartInfo();
            }
            psi.FileName = dotnetExecutable;

            var argumentList = new List<string>();
            argumentList.AddRange(new[] { "exec", "--runtimeconfig", runtimeConfig, "--depsfile", depsFile, thisAssembly,
                                               className, methodName, exceptionFile });
            if (args != null)
            {
                argumentList.AddRange(args);
            }

            psi.Arguments = string.Join(" ", argumentList);
            Diagnostics.Process process = Diagnostics.Process.Start(psi);

            return new RemoteExecution(process, className, methodName, exceptionFile);
        }

        private static string GetApplicationArgument(string name)
        {
            string[] arguments = GetApplicationArguments();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i] == name)
                {
                    return arguments[i + 1];
                }
            }
            return null;
        }

        private static string[] s_arguments;

        private static string[] GetApplicationArguments()
        {
            // Environment.GetCommandLineArgs doesn't include arguments passed to the runtime.
            // We use a native API to get all arguments.

            if (s_arguments != null)
            {
                return s_arguments;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                s_arguments = File.ReadAllText($"/proc/{Diagnostics.Process.GetCurrentProcess().Id}/cmdline").Split(new[] { '\0' });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.IntPtr ptr = GetCommandLine();
                string commandLine = Marshal.PtrToStringAuto(ptr);
                s_arguments = CommandLineToArgs(commandLine);
            }
            else
            {
                throw new PlatformNotSupportedException($"{nameof(GetApplicationArguments)} is not supported on this platform.");
            }

            return s_arguments;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetCommandLine();

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}
