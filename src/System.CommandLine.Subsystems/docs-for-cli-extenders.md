# Docs for folks extending System.CommandLine subsystem

There are a few ways to extend System.CommandLine subsystems

* Replace an existing subsystem, such as replacing Help.
* Add a new subsystem we did not implement.
* Supply multiple subsystems for an existing category, such as running multiple Help subsystems.

This design is based on the following assumptions:

* There will be between 10 and 10,000 CLI authors for every extender.
* There will be more replacement of existing subsystems than creation of new ones.
* CLI authors will often want to replace subsystems, especially help.
* Some folks will want extreme extensibility.
* Data needs to be exchanged between subsystems (this is the area of most significant change from prior versions).

We believe the space is fairly well understood, and that the subsystems we supply will cover most scenarios. However, we know of additional scenarios, such as prompting for required data.

Subsystems can be used with or outside the pipeline.

## Calling a subsystem without the pipeline

```mermaid
sequenceDiagram
  actor Author as CLI author
  participant Parser as Core parser
  participant Subsystem as Help subsystem
  participant App as Application

  Author->>Author: Creates CLI,<br/>includes Help option
  Author->>Parser: Creates parser
  Parser->>Author: returns parser
  Author->>Parser: Calls parser 'Parse', passing command line
  Parser->>Author: 'ParseResult'
  Author->>Author: Checks for '-h'
  alt Help requested
    Author->>Subsystem: Calls help
    Subsystem->>Author: 
  else Continue processing
    Author->>App: Calls application code
    App->>Author: Exit code
  end
```

## Subsystem calls with the pipeline, without invocation

```mermaid
sequenceDiagram
  actor Author as CLI author
  participant Pipeline as Pipeline
  participant Parser as Core parser
  participant Subsystem as Help subsystem
  participant OtherSubsystem as Other subsystems
  participant App as Application

  Author->>Author: Creates CLI,<br/>does not include Help option
  Author->>Pipeline: Creates pipeline
  Pipeline->>Author: 
  Author->>Pipeline: Calls parser 'Parse',<br/>passing command line
  Pipeline->>Subsystem: 'Initialize'
  Subsystem->>Subsystem: Adds '-h' to CLI
  Subsystem->>Pipeline: returns
  Pipeline->>Parser: Calls 'Parse'
  Parser->>Pipeline: returns 'ParseResult'
  Pipeline->>Subsystem: 'CheckIfActivated'
  Subsystem->>Subsystem: Checks for '-h'
  Subsystem->>Pipeline: True if '-h', otherwise false
  opt '-h', help requested
    Pipeline->>Subsystem: 'Execute'
    Subsystem->>Subsystem: Display help
    Subsystem->>Pipeline: Updated PipelineResult<br/>'AlreadyHandled' set to true
  end  
  loop For all configured subsystems
      Pipeline->>OtherSubsystem: `ExecuteIfNeeded`
      OtherSubsystem->>Pipeline: 
  end
  Pipeline->>Author: 'PipelineResult'
  Author->>Author: Check if  `AlreadyHandled`
  opt `AlreadyHandled` is false
    Author->>App: Calls application code
    App->>Author: Exit code
  end
```


## Subsystem calls with the pipeline, with invocation

```mermaid
sequenceDiagram
  actor Author as CLI author
  participant Pipeline as Pipeline
  participant Parser as Core parser
  participant Subsystem as Help subsystem
  participant Invoke as Invocation subsystem
  participant OtherSubsystem as Other subsystems
  participant App as Application

  Author->>Author: Creates CLI,<br/>does not include Help option
  Author->>Pipeline: Creates pipeline
  Pipeline->>Author: 
  Author->>Pipeline: Calls parser 'Parse',<br/>passing command line
  Pipeline->>Subsystem: 'Initialize'
  Subsystem->>Subsystem: Adds '-h' to CLI
  Subsystem->>Pipeline: returns
  Pipeline->>Parser: Calls 'Parse'
  Parser->>Pipeline: returns 'ParseResult'
  Pipeline->>Subsystem: 'CheckIfActivated'
  Subsystem->>Subsystem: Checks for '-h'
  Subsystem->>Pipeline: True if '-h', otherwise false
  opt '-h', help requested
    Pipeline->>Subsystem: 'Execute'
    Subsystem->>Subsystem: Display help
    Subsystem->>Pipeline: Updated PipelineResult<br/>'AlreadyHandled' set to true
  end  
  loop For all configured subsystems
      Pipeline->>OtherSubsystem: `ExecuteIfNeeded`
      OtherSubsystem->>Pipeline: 
  end
  Pipeline->>Pipeline: Check if  `AlreadyHandled`
  opt `AlreadyHandled` is false
    Pipeline->>Invoke: 'Execute'
    Invoke->>App: Runs application
    App->>Invoke: Exit code
    Invoke->>Pipeline: Updated 'PipelineResult'
  end
  Pipeline->>Author: 'PipelineResult'
```

## Replacing an existing subsystem or adding a new one

* Inherit from the existing subsystem or CliSubsystem
* Override `GetIsActivated`, unless your subsystem should never run (such as you have initialization only behavior):
  * You will generally not need to do this except for new subsystems that need to add triggers.
  * If your subsystem should run even if another subsystem has handled execution (extremely rare), set `ExecuteEvenIfAlreadyHandled`
* Override `Initialize` if needed:
  * You will generally not need to do this except for new subsystems that need to respond to their triggers.
  * Delay as much work as possible until it is actually needed.
* Override `Execute`:
  * Ensure that output is sent to `Console` on the pipeline, not directly to `StdOut`, `StdErr` or `System.Console`
* To manage data:
  * For every piece data value, create a public `Get...` and `Set...` method using the accessor pattern that allows  CLI authors to use the `With` extension method and implicitly converts to string (replace "Description" with the name of your data value in 6 places and possibly change the type in 2 places):

```csharp
    public void SetDescription(CliSymbol symbol, string description) 
        => SetAnnotation(symbol, HelpAnnotations.Description, description);

    public AnnotationAccessor<string> Description 
        => new(this, HelpAnnotations.Description);
```

* Let folks know to add your subsystem, or provide an alternative to StandardPipeline.


