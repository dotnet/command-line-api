
# Model Binding

Parsing command line arguments is a means to an end. You probably don't really want to think about parsing the command line. You just want some arguments passed to a method. 

In C#, the application entry point method has always looked something like this:

```cs
static void Main(string[] args)
{ 
}
```

The goal of every command line parsing library is to turn the string array passed to `Main` into something more useful. You might ultimately want to call a method that looks like this:

```cs
void Handle(int anInt)
{
}
```

So for example, you might want an input `"123"` from the command line to be converted into an `int` `123`. This conversion of command line input into variables or arguments you can use in your code is called "binding."

There are a number of ways do binding in `System.CommandLine`. The following tutorials go into detail on the different approaches available: