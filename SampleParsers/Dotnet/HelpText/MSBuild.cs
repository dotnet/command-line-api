namespace CommandLine.SampleParsers.Dotnet.HelpText
{
    public static  class MSBuild

    {
        public const string HelpText =
            @"Microsoft (R) Build Engine version 15.2.2.21747
Copyright (C) Microsoft Corporation. All rights reserved.

Syntax:              MSBuild.exe [options] [project file | directory]

Description:         Builds the specified targets in the project file. If
                     a project file is not specified, MSBuild searches the
                     current working directory for a file that has a file
                     extension that ends in ""proj"" and uses that file.  If
                     a directory is specified, MSBuild searches that
                     directory for a project file.

Switches:

  /target:<targets>  Build these targets in this project. Use a semicolon or a
                     comma to separate multiple targets, or specify each
                     target separately. (Short form: /t)
                     Example:
                       /target:Resources;Compile

  /property:<n>=<v>  Set or override these project-level properties. <n> is
                     the property name, and <v> is the property value. Use a
                     semicolon or a comma to separate multiple properties, or
                     specify each property separately. (Short form: /p)
                     Example:
                       /property:WarningLevel=2;OutDir=bin\Debug\

  /maxcpucount[:n]   Specifies the maximum number of concurrent processes to
                     build with. If the switch is not used, the default
                     value used is 1. If the switch is used without a value
                     MSBuild will use up to the number of processors on the
                     computer. (Short form: /m[:n])

  /toolsversion:<version>
                     The version of the MSBuild Toolset (tasks, targets, etc.)
                     to use during build. This version will override the
                     versions specified by individual projects. (Short form:
                     /tv)
                     Example:
                       /toolsversion:3.5

  /verbosity:<level> Display this amount of information in the event log.
                     The available verbosity levels are: q[uiet], m[inimal],
                     n[ormal], d[etailed], and diag[nostic]. (Short form: /v)
                     Example:
                       /verbosity:quiet

  /consoleloggerparameters:<parameters>
                     Parameters to console logger. (Short form: /clp)
                     The available parameters are:
                        PerformanceSummary--Show time spent in tasks, targets
                            and projects.
                        Summary--Show error and warning summary at the end.
                        NoSummary--Don't show error and warning summary at the
                            end.
                        ErrorsOnly--Show only errors.
                        WarningsOnly--Show only warnings.
                        NoItemAndPropertyList--Don't show list of items and
                            properties at the start of each project build.
                        ShowCommandLine--Show TaskCommandLineEvent messages
                        ShowTimestamp--Display the Timestamp as a prefix to any
                            message.
                        ShowEventId--Show eventId for started events, finished
                            events, and messages
                        ForceNoAlign--Does not align the text to the size of
                            the console buffer
                        DisableConsoleColor--Use the default console colors
                            for all logging messages.
                        DisableMPLogging-- Disable the multiprocessor
                            logging style of output when running in
                            non-multiprocessor mode.
                        EnableMPLogging--Enable the multiprocessor logging
                            style even when running in non-multiprocessor
                            mode. This logging style is on by default.
                        ForceConsoleColor--Use ANSI console colors even if
                            console does not support it
                        Verbosity--overrides the /verbosity setting for this
                            logger.
                     Example:
                        /consoleloggerparameters:PerformanceSummary;NoSummary;
                                                 Verbosity=minimal

  /noconsolelogger   Disable the default console logger and do not log events
                     to the console. (Short form: /noconlog)

  /fileLogger[n]     Logs the build output to a file. By default
                     the file is in the current directory and named
                     ""msbuild[n].log"". Events from all nodes are combined into
                     a single log. The location of the file and other
                     parameters for the fileLogger can be specified through
                     the addition of the ""/fileLoggerParameters[n]"" switch.
                     ""n"" if present can be a digit from 1-9, allowing up to
                     10 file loggers to be attached. (Short form: /fl[n])

  /fileloggerparameters[n]:<parameters>
                     Provides any extra parameters for file loggers.
                     The presence of this switch implies the
                     corresponding /filelogger[n] switch.
                     ""n"" if present can be a digit from 1-9.
                     /fileloggerparameters is also used by any distributed
                     file logger, see description of /distributedFileLogger.
                     (Short form: /flp[n])
                     The same parameters listed for the console logger are
                     available. Some additional available parameters are:
                        LogFile--path to the log file into which the
                            build log will be written.
                        Append--determines if the build log will be appended
                            to or overwrite the log file. Setting the
                            switch appends the build log to the log file;
                            Not setting the switch overwrites the
                            contents of an existing log file.
                            The default is not to append to the log file.
                        Encoding--specifies the encoding for the file,
                            for example, UTF-8, Unicode, or ASCII
                     Default verbosity is Detailed.
                     Examples:
                       /fileLoggerParameters:LogFile=MyLog.log;Append;
                                           Verbosity=diagnostic;Encoding=UTF-8

                       /flp:Summary;Verbosity=minimal;LogFile=msbuild.sum
                       /flp1:warningsonly;logfile=msbuild.wrn
                       /flp2:errorsonly;logfile=msbuild.err

  /distributedlogger:<central logger>*<forwarding logger>
                     Use this logger to log events from MSBuild, attaching a
                     different logger instance to each node. To specify
                     multiple loggers, specify each logger separately.
                     (Short form /dl)
                     The <logger> syntax is:
                       [<logger class>,]<logger assembly>[;<logger parameters>]
                     The <logger class> syntax is:
                       [<partial or full namespace>.]<logger class name>
                     The <logger assembly> syntax is:
                       {<assembly name>[,<strong name>] | <assembly file>}
                     The <logger parameters> are optional, and are passed
                     to the logger exactly as you typed them. (Short form: /l)
                     Examples:
                       /dl:XMLLogger,MyLogger,Version=1.0.2,Culture=neutral
                       /dl:MyLogger,C:\My.dll*ForwardingLogger,C:\Logger.dll

  /distributedFileLogger
                     Logs the build output to multiple log files, one log file
                     per MSBuild node. The initial location for these files is
                     the current directory. By default the files are called
                     ""MSBuild<nodeid>.log"". The location of the files and
                     other parameters for the fileLogger can be specified
                     with the addition of the ""/fileLoggerParameters"" switch.

                     If a log file name is set through the fileLoggerParameters
                     switch the distributed logger will use the fileName as a
                     template and append the node id to this fileName to
                     create a log file for each node.

  /logger:<logger>   Use this logger to log events from MSBuild. To specify
                     multiple loggers, specify each logger separately.
                     The <logger> syntax is:
                       [<logger class>,]<logger assembly>[;<logger parameters>]
                     The <logger class> syntax is:
                       [<partial or full namespace>.]<logger class name>
                     The <logger assembly> syntax is:
                       {<assembly name>[,<strong name>] | <assembly file>}
                     The <logger parameters> are optional, and are passed
                     to the logger exactly as you typed them. (Short form: /l)
                     Examples:
                       /logger:XMLLogger,MyLogger,Version=1.0.2,Culture=neutral
                       /logger:XMLLogger,C:\Loggers\MyLogger.dll;OutputAsHTML

  /warnaserror[:code[;code2]]
                     List of warning codes to treats as errors.  Use a semicolon
                     or a comma to separate multiple warning codes. To treat all
                     warnings as errors use the switch with no values.
                     (Short form: /err[:c;[c2]])

                     Example:
                       /warnaserror:MSB4130

                     When a warning is treated as an error the target will
                     continue to execute as if it was a warning but the overall
                     build will fail.

  /warnasmessage[:code[;code2]]
                     List of warning codes to treats as low importance
                     messages.  Use a semicolon or a comma to separate
                     multiple warning codes.
                     (Short form: /nowarn[:c;[c2]])

                     Example:
                       /warnasmessage:MSB3026

  /ignoreprojectextensions:<extensions>
                     List of extensions to ignore when determining which
                     project file to build. Use a semicolon or a comma
                     to separate multiple extensions.
                     (Short form: /ignore)
                     Example:
                       /ignoreprojectextensions:.sln

  /preprocess[:file]
                     Creates a single, aggregated project file by
                     inlining all the files that would be imported during a
                     build, with their boundaries marked. This can be
                     useful for figuring out what files are being imported
                     and from where, and what they will contribute to
                     the build. By default the output is written to
                     the console window. If the path to an output file
                     is provided that will be used instead.
                     (Short form: /pp)
                     Example:
                       /pp:out.txt

  /detailedsummary
                     Shows detailed information at the end of the build
                     about the configurations built and how they were
                     scheduled to nodes.
                     (Short form: /ds)

  @<file>            Insert command-line settings from a text file. To specify
                     multiple response files, specify each response file
                     separately.

                     Any response files named ""msbuild.rsp"" are automatically
                     consumed from the following locations:
                     (1) the directory of msbuild.exe
                     (2) the directory of the first project or solution built

  /noautoresponse    Do not auto-include any MSBuild.rsp files. (Short form:
                     /noautorsp)

  /nologo            Do not display the startup banner and copyright message.

  /version           Display version information only. (Short form: /ver)

  /help              Display this usage message. (Short form: /? or /h)

Examples:

        MSBuild MyApp.sln /t:Rebuild /p:Configuration=Release
        MSBuild MyApp.csproj /t:Clean
                             /p:Configuration=Debug;TargetFrameworkVersion=v3.5";
    }
}