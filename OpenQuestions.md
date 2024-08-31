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

Our default validations will be on the Condition for the shortcut. Users can offer alternatives by creaing custom validators. The dictionary for custom validators will be lazy, and lookups will be pay for play when the user has custom validators. (This is not yet implemented.)

When present, custom validators have precedence. There is no cost when they are not present.

## Should conditions be public

Since there are factory methods and validators could still access them, current behavior could be supported with internal conditions.

However, the point of conditions is that they are a statement about the symbol and not an implementation. They are known to be used by completions and completions are expected to be extended. Thus, to get the values held in the condition (such as environment variable name) need to be available outside the external scope.

Suggestion: Use internal constructors and leave conditions public

## Should `ValueCondition` be called `Condition`?

They may apply to commands.