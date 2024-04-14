# [Proposal] Output in Powderhouse

Output for the Powderhouse version of System.CommandLine will be used in these scenarios:

* Help
* Error reporting
* Optionally by applications using System.CommandLine

While it is reasonable to allow System.CommandLine apps to access its outputting system to provide consistent output, it is a non-goal to create a general outputting approach for .NET.

Output will go to a TextWriter, either the standard TextWriter or one derived from it that has overloads that take other types - such as the layout types described here and supports a formatter model to handle them. This alternate design has a few advantages, including moving the project forward without this work, and in the future allowing simple output to be simple.

Output will optionally allow outputting to multiple formats from a single definition, such as  output to plain text, fancy console, or markdown and orthogonally to multiple locations (console, files in different formats). This will result from multiple parts:

* Content
* Layout (Optional)
* Output/TextWriter
  * With formatting (Optional)

The content will be plain text, text with ANSI codes, System.CommandLine types, or subsystem types. The output will be a TextWriter, which is an explicit decision not to support binary output with this API. The rest of this proposal focuses on layout and formatting.

## Output

Output will be to a TextWriter. We anticipate a StreamWriter under the hood that can handle writing to StdOut/StdError, memory, files, etc. 

The TextWriter can be a FormattingTextWriter which can handle layout types, such as those described below. These will be created as they add value to the effort.

## Layout

Layout is tied to formatters, and are optional. Code can write directly to the TextWriter.

Layout will be via an open set of blocks:

* TextBlock
* Section
* Table
* Usage
* Code

The first four block types are general and might be useful in an application, and thus will be available to System.CommandLine apps to let CLI authors supply consistent output to end users. Usage is not expected to be used outside System.CommandLine help and represents the kind of specific block which might be useful in other situations. For example, if a CLI author needed to output addresses, they could create an AddressBlock.

Blocks can be nested, such as the Section block type.

All blocks inherit from an abstract Block type.

> _Question:_ Should we allow any Block to be nested, and thus have a Children property on all blocks, or only Section and its derived blocks. IOW, do we want to allow things like a table within a footnote, or sections within a table cell.

The construction of each block type will be different. As some potential examples:

```csharp
var newTable = new Table<CliOption>("Table title", command.Arguments); // also configuration
newTable.AddColumn("Name", argument => argument.Name); 
newTable.AddColumn("Description", argument => help.GetDescription(argument));
var newSection = new Section("Arguments", newTable);
```

The lambdas here return a Block or a TextBlock.

> _Question:_ How should we provide subsystems - the above approach has a closure on help. We could pre-create the data (a two dimensional array for a table). Since this would only be an issue where a lambda is needed, it may be an issue only for tables, in which case maybe pre-creating the data is not a terrible idea.

### Block API

Blocks will have at least the ability to output.

```csharp
public abstract class Block
   public Block(object data);
   public object Data { get; }
   // block specific data
```

> _Question:_ What should be returned? A string or something that would allow images. For System.CommandLine is there a need for images, other than the display of an image separately created via a reference. _Tentative answer: a string is fine. We care most about markdown, terminal and HTML._

### Rich text

To provide rich text, such as color and emphasis, we need abstractions for defining this. Formatters may output specific block types in a specific way, such as a section header output with escape code to the console, `# section title` to markdown, a CSS class for HTML, or nothing for a file. That should "just work". However, within a TextBlock, we can only display richness if we can define it.

In addition to emphasis and color, abstractions such as error or heading may be desirable. Hyperlinks would be highly desirable.

We can anticipate that folks may come up with alternate approaches, which is fine as long as there is a formatter available.

The approach we take must allow replacement of the rich text definition per formatter. For example, it might be a search and replace (but probably harder to do).

Simplistically, there seem to be two approaches: start/start something else and start/end. For example the terminal has an escape code to underline something, and a different code to return to normal. HTML surrounds with start/end, and markdown uses the same character for start and end with rules. Perhaps only start/end can be turned into the other two approaches.

> _More work needed on this_

### Code

There is a general request for examples for the .NET CLI and many CLIs include help. It would be quite nice to be able to output code, although that is not needed to match existing features.

### Footnotes

We may consider adding a Footnote block type, because it will save folks trying to figure out the pattern. It's just a key/string pair whose key is determined by the Footnote set and available immediately after a footnote is created so that it can be inserted in that text. The key is usually the order in which the items were added. Later the collected footnotes are outputted with their keys.

We don't need this at GA. It might be a nice to have sometimes, especially if the .NET CLI or `dotnet new` can use it to simplify some gnarly displays.

### Table of Contents

Blocks need to be able to peruse the tree of other blocks. Right now the only use I see is for creating a Table of Contents.

### Section headings

Sections are pretty much a title and a set of blocks.

They will not contain a heading level. The level will be determined by the position in the tree of blocks. The scenario where this is preferable is help where the information will generally be output just for a command, and thus a heading like _Usage_ would be equivalent to an Heading 1 in Word or a `##` in markdown. The information may also be available for documentation in output that contains all of the commands, and possibly other information. In this case, a heading like _Usage_ would be a Heading 2 or `###`. To avoid complexities from this, the level of section heading will be determined from their depth in the tree.

### How specific should blocks be?

For example, should the table for errors, options, and arguments (or commands in documentation/help for the full system), or should they each have their own block type. Formatters are expected to work via pattern matching, so in the normal case, it would make no difference. It can vary by help system and overtime can become more specific. So, this is really just asking whether we should start with a symbol specific types. Seems reasonable.

## Formatters

Formatters are needed when layout blocks are used, and otherwise optional. They are managed by a TextWriter.

Scenarios are:

* Formatters that implement a consistent look and supplies output for some or all known block types for a specific output look (such as Spectre)
* Formatters that override a specific block type for a custom look (such as customizing all tables)
* Formatters that override a specific block (such as customizing only the option block)

Formatters are per block type, or block. This is important to allow customization of details without needing to copy the entire formatter. It also means that the current formatter may not support a block when another formatter does. That is because, as this is written, the current formatter is the one for the parent.

> _Question for implementation:_ Should the current formatter have precedence? As this code is written, the current formatter does have precedence. If the conditional for CanHandle within this method is removed, such that formatters.Output is always called, then the current formatter does not have precedence. Thought example: A plain text formatter can supply adequate output for a section title. If the current formatter has precedence, then a terminal formatter would be required to implement sections, or output of the child blocks would be done by the plain text formatter instead of the terminal formatter. OTOH, having to peruse the formatters for every block sounds inefficient, and intuitively, it seems that sticking to the same formatter would result in a more consistent look. Alternative designs would include having a preferred formatter and using it if possible, creating a dictionary of block type and the best formatter, although this would not allow a different formatter for block data contents, or something else.

### No op

A formatter can emit nothing for a block. An example of when this might be helpful is if output is going to a file, titles may be excluded.

### Capabilities

Terminals/consoles vary in their capabilities. This issue is not expected to exist for other output formats.

This will probably result in a need to initialize terminal/console formatters with information about available capabilities.
