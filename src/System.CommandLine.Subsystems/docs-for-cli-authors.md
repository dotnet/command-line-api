# Docs for CLI authors

The "grow-up" story for CLIs. 
 
* A CLI author may begin a project thinking they will never need more than simply parsing, and won't have subcommands. That's great, they can use the core parser without the subsystem layer. They will retrieve data from the ParseResult and features, including default values, will not be available
* The CLI author decides they want one or a couple of features of the subsystem layer - we think it will probably be help or default values, but it might be tab completion (see note on Validation below). They can explicitly call a subsystem, although the current API will be simplified.
* The CLI author decides that they want all available functionality. They can use the pipeline without invocation if they would prefer to map values to parameters and invoke themselves. 
* The CLI author winds up with several subcommands and determining which code to call and mapping becomes a bit messy or a burden. At this point, they can add invocation.

Of course folks may jump into this flow anywhere, and all four scenarios will be supported. The need for this flexibilty is one of the things we learned from feedback on existing main (the current preview of System.CommandLine that we are replacing).

## Basic usage for full featured parser

* Create a custom parser by adding options, arguments and commands
  * The process of adding options, arguments and commands is the same for the parser and subcommands
* Run StandardPipeline.Execute(args)