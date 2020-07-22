# Functional goals

The high level goals for `System.CommandLine` support our idea that creating great command line experiences for your users can be easy. We have not yet met all of these goals.

## Goals for the end user's experience

* Help should be clear and consistent
  * I should not have to know the right syntax to get help. Just make all variations work.
* Tab suggestion should just work with as little setup as possible.

## Goals for the programmer's experience

* Help
  
  * Creating some version of help should be automated, requiring no work.
  * Help should be informative if descriptions are supplied.
  * Help localization should be straightforward.

* Tab suggestion
  
  * It should just work with no effort on the programmer's part.
  * It should provide support for enums.
  * It should have a mechanism for dynamic values.
  * It should stay out of the way of shell suggestions for files and folders.
  * Dynamic suggestions should be extensible for other things I might do.

* Validation
  
  * If there are parse errors, it should fail before my application code is called.
  * It should check the number and type of arguments.
  * It should generally fail if there are unmatched tokens, but I should be able to allow it to pass through.
  * It should provide default responses for the user on validation issues.
  * I should be able to customize the validation messages.

* Debugging and testing
  
  * I should not have to turn a string into an array to interact programmatically.
  * I should be able to get a visualization of how a string is parsed.
  * It should be easy to test parsing in isolation from the application.
  * It should be easy to test the application in isolation from parsing.
  * I should be able to specify at the command line that I want to attach a debugger.

* Acting on parser results
  
  * Argument results should be strongly typed.
  * For advanced scenarios, I can alter and re-parse input.
  * It should be simple for me to manage exceptions, output, and exit codes.

* Rendering
  
  * Provide ways to reason about layout rather than just text.
  * Hide Windows/Linux/Mac differences for me.
  * Take advantage of new Windows 10 console capabilities.
  * Hide non-ANSI/ANSI differences for me.
  * Make output look correct when redirected to a file.

* Extensibility
  
  * I can compose cross-cutting behaviors using packages.

