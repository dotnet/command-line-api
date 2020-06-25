# Quickstart - binding to methods with options objects

Basic scenario - the program has several "commands"/"verbs" each with its
set of options.

```cs
class Program {
  public static void Main(string[] args) {
    var cmdLine = CreateCommands();
    cmdLine.Invoke(args);
  }
  
  private static RootCommand CreateCommands() {
    var root = new RootCommand();

    var list = new Command("list", "list files in dir");
    list.AddArgument(new Argument<DirectoryInfo>("input", "path to existing file/dir").ExistingOnly());
    list.Handler = CommandHandler.Create((OptsCmdList opts) => RunCmdList(opts));
    
    var info = new Command("info", "show info");
    info.AddOption(new Option<string>(new[]{"-a", "--aaa"}){Required = true});
    info.AddOption(new Option<string>(new[]{"-b", "--bbb"}){Required = false});
    info.AddOption(new Option<string>(new[]{"-c", "--ccc"}){Required = false, Name = "binding-by-name"});
    info.Handler = CommandHandler.Create((OptsCmdInfo opts) => RunCmdInfo(opts));

    root.AddCommand(list);
    root.AddCommand(info);

    return root;
  }
  
  public static void RunCmdList(OptsCmdList opts) {
    Console.WriteLine($"RunCmdList launched with options:\n{opts}");
    Console.WriteLine($"Dir: ${opts.Input}");
    foreach (var fn in opts.Input.GetFiles().Select(fi => fi.Name)) {
      Console.WriteLine($"\tFile: {fn}");
    }
  }

  public static void RunCmdInfo(OptsCmdInfo opts) {
    Console.WriteLine($"RunCmdInfo launched with options:\n{opts}");
  }
}

class OptsCmdList {
  public DirectoryInfo Input { get; set; }

  public override string ToString() {
    return $"{nameof(Input)}: {Input}";
  }
}

class OptsCmdInfo {
  public String Aaa { get; set; }
  public String B { get; set; }
  public String BindingByName { get; set; }

  public override string ToString() {
    return $"{nameof(Aaa)}: {Aaa}, {nameof(B)}: {B}, {nameof(BindingByName)}: {BindingByName}";
  }
}
```
