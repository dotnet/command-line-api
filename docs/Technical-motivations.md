# Technical motivations

* Provide a parser that's independent of binding and invocation.
  
  * You can create your own conventions and your own API surface.
  * Parsing can easily be tested independently from other application logic.
  * The syntax abstraction opens up use cases such as an extensible completions model.
  * Features such as syntax validation can be customized independent of your API.
  * Grammars can be serialized for interpretation by other tools.
  * Different APIs using this base layer can interoperate easily.
  * Don't always assume a console app entry point: provide `Main(string[])`-equivalent string splitting.

* Make the end user command line experience consistent and inclusive.
  
  * Support both POSIX and Windows syntax conventions.
  * Make help easy to find by defaulting into all common conventions (`-h`, `--help`, `/h` `/?`, `-?`).
  * Make completions work by default, so that they become the norm.
  * Make it possible to write completion providers for non-.NET command line applications.
  * Localization support for all messages that the library emits.
  * Parse diagramming, to help people understand a program's syntax without invoking it, is available by default.

* A composable chain of responsibility for subcommand routing, middleware, directives, etc.
  
  * An invocation model that allows for short-circuiting and interception, with access to the parser, meaning cross-cutting behaviors can be composed into your app via NuGet packages.
  * Simplify debugging by providing an interception hook in the middleware pipeline.
  * Support for handling process cancellation.
  * Directive syntax provides a consistent extensibility point that does not interfere with your app's syntax.
  * Supports command line API versioning via directives.

* Rich, adapative output rendering
  
  * Write output code once and render it correctly based on the presence or absence of a terminal as well as terminal capabilities.
  * Support for higher-level layouts, tables, event-based re-rendering, and animation.
  * Support for standard render mode hints via directives.

* Other things:
  
  * Support arbitrarily deep nesting of subcommands.
  * Support response files.
  * Restore console state after application exit.

