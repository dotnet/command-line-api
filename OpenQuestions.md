# Open questions

Please also include TODO in appropriate locations. This is intended as a wrap up.

Also, include whether the question is to add or remove something, and add date/initials

## NotNulWhen on TryGetValue in ValueSource and ValueProvider

Things to consider:

* Within System.CommandLine all TryGetValue should probably be the same
* TryGetValue on dictionary can return null
* For nullable values, the actual value can be null
* For nullable or non-nullable ref types, the default for the type is null
* Allowing null out values keeps a single meaning to "not found" and allows "found but null". Conflating these blocks expressing which happened 

The recovery is the same as with Dictionary.TryGetValue. The first line of the block that handles the return Boolean is a guard.

## The extensibility story for ValueSource

The proposal and current code seal our value sources and expect people to make additional ones based on ValueSource. The classes are public and sealed, the constructors are internal.

Reasons to reconsider: Aggregate value source has a logical precedence or an as entered one. If someone adds a new value source, it is always last in the logic precedence.There are likely to be other similar cases.

Possible resolution: Have this be case by case and allow aggregate values to be unsealed and have a mechanism for overriding. Providing a non-inheritance based solution could make this look like a normal operation when it is a rare one.

## Contexts [RESOLVED]

We had two different philosophies at different spots in subsystems. "Give folks everything they might need" and "Give folks only what we know they need".

The first probably means we pass around `PipelineResult`. The second means that each purpose needs a special context. Sharing contexts is likely to mean that something will be added to one context that is unneeded by the other. Known expected contexts are:

- `AnnotationProviderContext` 
- `ValueSourceContext` 
- `ValidationContext` (includes ability to report diagnostics)
- `CompletionContext` 
- `HelpContext` 

## Which contexts should allow diagnostic reporting?

## Should we have both Validators and IValidator on Conditions? [RESOLVED]

We started with `Validators` and then added the IValidator interface to allow conditions to do validation because they have the strong type. Checking for this first also avoids a dictionary lookup.

Our default validations will be on the Condition for the shortcut. Users can offer alternatives by creating custom validators. The dictionary for custom validators will be lazy, and lookups will be pay for play when the user has custom validators. (This is not yet implemented.)

When present, custom validators have precedence. There is no cost when they are not present.

## Should conditions be public

Since there are factory methods and validators could still access them, current behavior could be supported with internal conditions.

However, the point of conditions is that they are a statement about the symbol and not an implementation. They are known to be used by completions and completions are expected to be extended. Thus, to get the values held in the condition (such as environment variable name) need to be available outside the external scope.

Suggestion: Use internal constructors and leave conditions public

## Should `ValueCondition` be called `Condition`?

They may apply to commands.

## Can we remove the "Annotations" in xxxxAnnotationExtensions

We have other extensions, such as `AddCalculation`. Where should it go?

They may shift to extension types in the future.

It's a long in Solution Explorer

## Calculated value design

My first direction on the calculated value design was to derive from CliSymbol and treat them similarly to any other CliSymbol. This results in a technical challenge in the way the `Add` method works for CliSymbols - specifically it does not allow adding anything except options and arguments and the design results in infinite recursion if the exception is ignored. While we might be able to finesse this, it indicates just how thing the ice is if we try to "trick" things in the core parser layer. 

Instead calculated values are a new thing. They can contribute symbols when asked - their internal components can be expressed as symbols for help, for example. However, they are not a CliSymbol and for all uses must be separately treated. 

They are held on commands via annotations. Calculated values that should be are not logically located on a symbol should be on the root command.

This will use collection annotations when they are available. For now they are List<CalculatedValue>.

We have a naming challenge that may indicate an underlying need to refactor:

- ValueSource: Knows how to get data from disparate sources - constants, other symbols, environment variables.
- Calculation: Parameter/property on ValueSources allowing them to be relative to their source
- CalculatedValue (possibly CliCalculatedValue): A new thing that can be declared by the CliAuthor for late interpretation and type conversions.
- ValueCondition, ValueSymbol and other places where "Value" allows unification of Option and Argument (and is very, very helpful for that)

## Tests that do not have `InternalsVisibleTo`

Currently all the code we have is in the `internal` scope within the logical layer. As a result, we are not testing how the libary works from the outside. We could take either of these two approaches or do something else, but I think we need to do something soon to validate our design:

- Isolate the very small amount of code that actually needs IVT into an additional test assembly for each logical layer. 
  - This would result in two new projects and IVT would be removed from the current test project.
  - The benefit of this approach is that the largest amount of code would be running without IVT.
- Create "example" projects:
  - This would result in one or two new projects, two if the dependencies are those that you would use when accessing either the raw parsing behavior or subsystems. Current tests would remain under IVT.
  - The benefit of this approach is that we will be creating functional tests in the form of "how do I do xxx". Since they are tests, they would not get out of date with the code base.
  - Another potential benefit would be a bounded space where we could focus on how our APIs work in practice.