// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using FluentAssertions.Equivalency;
using System.Linq;
using FluentAssertions.Common;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        private readonly ITestOutputHelper _output;

        public ParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void An_option_without_a_long_form_can_be_checked_for_using_a_prefix()
        {
            var result = new Parser(
                    new Option("--flag"))
                .Parse("--flag");

            result.HasOption("--flag").Should().BeTrue();
        }

        [Fact]
        public void An_option_can_be_checked_by_object_instance()
        {
            var option = new Option("--flag");
            var option2 = new Option("--flag2");
            var result = new Parser(option, option2)
                .Parse("--flag");

            result.HasOption(option).Should().BeTrue();
            result.HasOption(option2).Should().BeFalse();
        }

        [Fact]
        public void An_option_without_a_long_form_can_be_checked_for_without_using_a_prefix()
        {
            var result = new Parser(
                    new Option("--flag"))
                .Parse("--flag");

            result.HasOption("flag").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_short_form_an_option_with_an_alias_can_be_checked_for_by_its_short_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }))
                .Parse("-o");

            result.HasOption("o").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_long_form_an_option_with_an_alias_can_be_checked_for_by_its_short_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }))
                .Parse("--one");

            result.HasOption("o").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_short_form_an_option_with_an_alias_can_be_checked_for_by_its_long_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }))
                .Parse("-o");

            result.HasOption("one").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_long_form_an_option_with_an_alias_can_be_checked_for_by_its_long_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }))
                .Parse("--one");

            result.HasOption("one").Should().BeTrue();
        }

        [Fact]
        public void Two_options_are_parsed_correctly()
        {
            ParseResult result = new Parser(
                    new Option(
                        new[] { "-o", "--one" }),
                    new Option(
                        new[] { "-t", "--two" })
                )
                .Parse("-o -t");

            result.HasOption("o").Should().BeTrue();
            result.HasOption("one").Should().BeTrue();
            result.HasOption("t").Should().BeTrue();
            result.HasOption("two").Should().BeTrue();
        }

        [Fact]
        public void Parse_result_contains_arguments_to_options()
        {
            var parser = new Parser(
                new Option(new[] { "-o", "--one" })
                {
                    Argument = new Argument { Arity = ArgumentArity.ExactlyOne }
                },
                new Option(new[] { "-t", "--two" })
                {
                    Argument = new Argument { Arity = ArgumentArity.ExactlyOne }
                });

            var result = parser.Parse("-o args_for_one -t args_for_two");

            result["one"].Arguments.Single().Should().Be("args_for_one");
            result["two"].Arguments.Single().Should().Be("args_for_two");
        }

        [Fact]
        public void When_no_options_are_specified_then_an_error_is_returned()
        {
            Action create = () => new Parser(Array.Empty<Symbol>());

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("You must specify at least one option or command.");
        }

        [Fact]
        public void Two_options_cannot_have_conflicting_aliases()
        {
            Action create = () =>
                new Parser(new Option(
                               new[] { "-o", "--one" }),
                           new Option(
                               new[] { "-t", "--one" }));

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("Alias '--one' is already in use.");
        }

        [Fact]
        public void A_double_dash_delimiter_specifies_that_no_further_command_line_args_will_be_treated_as_options()
        {
            var result = new Parser(new Option(new[] { "-o", "--one" }))
                .Parse("-o \"some stuff\" -- -x -y -z -o:foo");

            result.HasOption("o")
                  .Should()
                  .BeTrue();

            result.UnparsedTokens
                  .Should()
                  .BeEquivalentSequenceTo("-x",
                                          "-y",
                                          "-z",
                                          "-o:foo");
        }

        [Fact]
        public void The_portion_of_the_command_line_following_a_double_dash_is_accessible_as_UnparsedTokens()
        {
            var result = new Parser(new Option("-o"))
                .Parse("-o \"some stuff\" -- x y z");

            result.UnparsedTokens
                  .Should()
                  .BeEquivalentSequenceTo("x", "y", "z");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_equals_delimiter()
        {
            var parser = new Parser(new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            });

            var result = parser.Parse("-x=some-value");

            result.Errors.Should().BeEmpty();

            result["x"].Arguments.Should().ContainSingle(a => a == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_equals_delimiter()
        {
            var parser = new Parser(
                new Option("--hello")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                });

            var result = parser.Parse("--hello=there");

            result.Errors.Should().BeEmpty();

            result["hello"].Arguments.Should().ContainSingle(a => a == "there");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_colon_delimiter()
        {
            var parser = new Parser(
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                });

            var result = parser.Parse("-x:some-value");

            result.Errors.Should().BeEmpty();

            result["x"].Arguments.Should().ContainSingle(a => a == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new Option("--hello")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };
            
            var result = option.Parse("--hello:there");

            result.Errors.Should().BeEmpty();

            result["hello"].Arguments.Should().ContainSingle(a => a == "there");
        }

        [Fact]
        public void Option_short_forms_can_be_bundled()
        {
            var command = new Command("the-command");
            command.AddOption(new Option("-x"));
            command.AddOption(new Option("-y"));
            command.AddOption(new Option("-z"));

            var result = command.Parse("the-command -xyz");

            result.CommandResult
                  .Children
                  .Select(o => o.Name)
                  .Should()
                  .BeEquivalentTo("x", "y", "z");
        }

        [Fact]
        public void Options_short_forms_do_not_get_unbundled_if_unbundling_is_turned_off()
        {
            var parser = new CommandLineBuilder()
                         .EnablePosixBundling(false)
                         .AddCommand(new Command("the-command")
                                     {
                                         new Option("-x"),
                                         new Option("-y"),
                                         new Option("-z")
                                     })
                         .Build();

            var result = parser.Parse("the-command -xyz");

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("-xyz");
        }

        [Fact]
        public void Option_long_forms_do_not_get_unbundled()
        {
            var parser = new Parser(
                new Command("the-command")
                {
                    new Option("--xyz"),
                    new Option("-x"),
                    new Option("-y"),
                    new Option("-z")
                });

            var result = parser.Parse("the-command --xyz");

            result.CommandResult
                  .Children
                  .Select(o => o.Name)
                  .Should()
                  .BeEquivalentTo("xyz");
        }

        [Fact]
        public void Options_do_not_get_unbundled_unless_all_resulting_options_would_be_valid_for_the_current_command()
        {
            var outer = new Command("outer");
            outer.AddOption(new Option("-a"));
            var inner = new Command("inner")
                        {
                            Argument = new Argument
                                       {
                                           Arity = ArgumentArity.ZeroOrMore
                                       }
                        };
            inner.AddOption(new Option("-b"));
            inner.AddOption(new Option("-c"));
            outer.AddCommand(inner);

            var parser = new Parser(outer);

            ParseResult result = parser.Parse("outer inner -abc");

            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo("-abc");
        }

        [Fact]
        public void Required_option_arguments_are_not_unbundled()
        {
            var optionA = new Option("-a")
            {
                Argument = new Argument<string>()
            };
            var optionB = new Option("-b");
            var optionC = new Option("-c");

            
            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-a -bc");

            result.FindResultFor(optionA)
                  .Tokens
                  .Should()
                  .ContainSingle(t => t.Value == "-bc");
        }

        [Fact]
        public void Optional_option_arguments_are_unbundled()
        {
            var optionA = new Option("-a")
            {
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };
            var optionB = new Option("-b");
            var optionC = new Option("-c");
            
            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-a -bc");

            result.Tokens
                  .Select( t => t.Value)
                  .Should()
                  .BeEquivalentTo("-a", "-b", "-c");
        }

        [Theory]
        [InlineData("-abcvalue", null, "value")]
        [InlineData("-abc value", null, "value")]
        [InlineData("-abc=value", null, "value")]
        [InlineData("-abc:value", null, "value")]
        [InlineData("-abvalue", "value", null)]
        [InlineData("-ab value", "value", null)]
        [InlineData("-ab=value", "value", null)]
        [InlineData("-ab:value", "value", null)]
        [InlineData("-abvalue -cvalue", "value", "value")]
        // a should always be set
        // b should always be set but value is optional
        // c is optional
        public void Last_bundled_option_can_accept_arguments(string arguments, string bvalue, string cvalue)
        {
            var optionA = new Option("-a");
            var optionB = new Option("-b")
            {
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };
            var optionC = new Option("-c") 
            {
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse(arguments);
            result.HasOption(optionA).Should().BeTrue();
            result.HasOption(optionB).Should().BeTrue();

            if (bvalue != null)
            {
                result.FindResultFor(optionB)
                    .Tokens
                    .Should()
                    .ContainSingle(t => t.Value == bvalue);
            }

            if (cvalue != null)
            {
                result.FindResultFor(optionC)
                    .Tokens
                    .Should()
                    .ContainSingle(t => t.Value == cvalue);
            }
            else
            {
                result.HasOption(optionC).Should().BeFalse();
            }
        }

        [Fact]
        public void Parser_root_Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var parser = new Parser(
                new Option(new[] { "-a", "--animals" })
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }, Description = ""
                },
                new Option(new[] { "-v", "--vegetables" })
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }, Description = ""
                });

            var result = parser.Parse("-a cat -v carrot -a dog");

            result["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat", "dog");

            result["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");
        }

        [Fact]
        public void Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var parser = new Parser(
                new Command("the-command") {
                    new Option(new[] { "-a", "--animals" })
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }.FromAmong("dog", "cat", "sheep"), Description = ""
                    },
                    new Option(new[] { "-v", "--vegetables" })
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }, Description = ""
                    }
                });

            var result = parser.Parse("the-command -a cat -v carrot -a dog");

            var command = result.CommandResult;

            command["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat", "dog");

            command["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");
        }

        [Fact]
        public void When_a_Parser_root_option_is_not_respecified_but_limit_is_not_reached_then_the_following_token_is_used_as_value()
        {
            var parser = new Parser(
                new Option(new[] { "-a", "--animals" })
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }, Description = ""
                },
                new Option(new[] { "-v", "--vegetables" })
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }, Description = ""
                });

            ParseResult result = parser.Parse("-a cat dog -v carrot");

            result["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo(new[]{"cat", "dog"});

            result["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");

            result
                .UnmatchedTokens
                .Should()
                .BeNullOrEmpty();
        }

        [Fact]
        public void When_a_Parser_root_option_is_not_respecified_and_limit_is_reached_then_the_following_token_is_unmatched()
        {
            var parser = new Parser(
                new Option(new[] { "-a", "--animals" })
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }, Description = ""
                },
                new Option(new[] { "-v", "--vegetables" })
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }, Description = ""
                });

            ParseResult result = parser.Parse("-a cat some-arg -v carrot");

            result["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat");

            result["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");

            result
                .UnmatchedTokens
                .Should()
                .BeEquivalentTo("some-arg");
        }

        [Fact]
        public void When_an_option_is_not_respecified_but_limit_is_not_reached_then_the_following_token_is_considered_as_value()
        {
            var parser = new Parser(
                new Command("the-command")
                {
                    new Option(new[] { "-a", "--animals" })
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }
                    },
                    new Option(new[] { "-v", "--vegetables" })
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }
                    }
                },
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                });

            ParseResult result = parser.Parse("the-command -a cat dog -v carrot");

            var command = result.CommandResult;

            command["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat", "dog");

            command["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");

            command
                .Arguments
                .Should()
                .BeNullOrEmpty();
        }

        [Fact]
        public void When_an_option_is_not_respecified_but_limit_is_reached_then_the_following_token_is_considered_an_argument_to_the_parent_command()
        {
            var parser = new Parser(
                new Command("the-command")
                {
                    new Option(new[] { "-a", "--animals" })
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ZeroOrOne
                        }
                    },
                    new Option(new[] { "-v", "--vegetables" })
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }
                    },
                    new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }});

            ParseResult result = parser.Parse("the-command -a cat some-arg -v carrot");

            var command = result.CommandResult;

            command["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat");

            command["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");

            command
                .Arguments
                .Should()
                .BeEquivalentTo("some-arg");
        }

        [Fact]
        public void Command_with_multiple_options_is_parsed_correctly()
        {
            var option = new Command("outer")
            {
                new Option("--inner1")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                },
                new Option("--inner2")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                }
            };

            var parser = new Parser(option);

            var result = parser.Parse("outer --inner1 argument1 --inner2 argument2");

            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     o.Name == "inner1" &&
                                     o.Arguments.Single() == "argument1");
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     o.Name == "inner2" &&
                                     o.Arguments.Single() == "argument2");
        }

        [Fact]
        public void Relative_order_of_arguments_and_options_within_a_command_does_not_matter()
        {
            var command = new Command("move")
            {
                new Argument<string[]>(),
                new Option("-X")
                {
                    Argument = new Argument<string>()
                }
            };

            // option before args
            ParseResult result1 = command.Parse(
                "move -X the-arg-for-option-x ARG1 ARG2");

            // option between two args
            ParseResult result2 = command.Parse(
                "move ARG1 -X the-arg-for-option-x ARG2");

            // option after args
            ParseResult result3 = command.Parse(
                "move ARG1 ARG2 -X the-arg-for-option-x");

            // all should be equivalent
            result1.Should()
                   .BeEquivalentTo(
                       result2,
                       x => x.IgnoringCyclicReferences()
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
            result1.Should()
                   .BeEquivalentTo(
                       result3,
                       x => x.IgnoringCyclicReferences()
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
        }

        [Theory]
        [InlineData("--one 1 --many 1 --many 2")]
        [InlineData("--one 1 --many 1 --many 2 arg1 arg2")]
        [InlineData("--many 1 --one 1 --many 2")]
        [InlineData("--many 2 --many 1 --one 1")]
        [InlineData("[parse] --one 1 --many 1 --many 2")]
        [InlineData("--one \"stuff in quotes\" this-is-arg1 \"this is arg2\"")]
        [InlineData("not a valid command line --one 1")]
        public void Original_order_of_tokens_is_preserved_in_ParseResult_Tokens(string commandLine)
        {
            var rawSplit = commandLine.SplitCommandLine();

            var command = new Command("the-command")
                          {
                              new Argument<string[]>(),
                              new Option("--one")
                              {
                                  Argument = new Argument<string>()
                              },
                              new Option("--many")
                              {
                                  Argument = new Argument<string[]>()
                              }
                          };

            var result = command.Parse(commandLine);

            result.Tokens.Select(t => t.Value).Should().Equal(rawSplit);
        }

        [Fact]
        public void An_outer_command_with_the_same_name_does_not_capture()
        {
            var command = new Command("one")
                          {
                              new Command("two")
                              {
                                  new Command("three")
                              },
                              new Command("three")
                          };

            ParseResult result = command.Parse("one two three");

            result.Diagram().Should().Be("[ one [ two [ three ] ] ]");
        }

        [Fact]
        public void An_inner_command_with_the_same_name_does_not_capture()
        {
         var command = new Command("one")
                          {
                              new Command("two")
                              {
                                  new Command("three")
                              },
                              new Command("three")
                          };

            ParseResult result = command.Parse("one three");

            result.Diagram().Should().Be("[ one [ three ] ]");
        }

        [Fact]
        public void When_nested_commands_all_accept_arguments_then_the_nearest_captures_the_arguments()
        {
            var command = new Command(
                "outer")
            {
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                },
                new Command("inner")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
                }
            };

            var result = command.Parse("outer arg1 inner arg2");

            result.CommandResult.Parent.Arguments.Should().BeEquivalentTo("arg1");

            result.CommandResult.Arguments.Should().BeEquivalentTo("arg2");
        }

        [Fact]
        public void Nested_commands_with_colliding_names_cannot_both_be_applied()
        {
            var command = new Command("outer")
            {
                new Argument<string>(),
                new Command("non-unique")
                {
                    new Argument<string>()
                },
                new Command("inner")
                {
                    new Argument<string>(),
                    new Command("non-unique")
                    {
                        new Argument<string>()
                    }
                }
            };

            ParseResult result = command.Parse("outer arg1 inner arg2 non-unique arg3 ");

            result.Diagram().Should().Be("[ outer <arg1> [ inner <arg2> [ non-unique <arg3> ] ] ]");
        }

        [Fact]
        public void When_child_option_will_not_accept_arg_then_parent_can()
        {
            var command = new Command("the-command")
                         {
                             new Option("-x"),
                             new Argument<string>()
                         };

            var result = command.Parse("the-command -x the-argument");

            _output.WriteLine(result.ToString());

            result.CommandResult["x"].Arguments.Should().BeEmpty();
            result.CommandResult.Arguments.Should().BeEquivalentTo("the-argument");
        }

        [Fact]
        public void When_parent_option_will_not_accept_arg_then_child_can()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument<string>()
                }
            };

            var result = command.Parse("the-command -x the-argument");

            result.CommandResult["x"].Arguments.Should().BeEquivalentTo("the-argument");
            result.CommandResult.Arguments.Should().BeEmpty();
        }

        [Fact]
        public void When_the_same_option_is_defined_on_both_outer_and_inner_command_and_specified_at_the_end_then_it_attaches_to_the_inner_command()
        {
            var outer = new Command("outer")
                        {
                            new Command("inner")
                            {
                                new Option("-x")
                            },
                            new Option("-x")
                        };

            ParseResult result = outer.Parse("outer inner -x");

            result.CommandResult
                  .Parent
                  .Children
                  .Should()
                  .NotContain(o => o.Name == "x");
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o => o.Name == "x");
        }

        [Fact]
        public void When_the_same_option_is_defined_on_both_outer_and_inner_command_and_specified_in_between_then_it_attaches_to_the_outer_command()
        {
            var outer = new Command("outer");
            outer.AddOption(new Option("-x"));
            var inner = new Command("inner");
            inner.AddOption(new Option("-x"));
            outer.AddCommand(inner);

            var result = outer.Parse("outer -x inner");

            result.CommandResult
                  .Children
                  .Should()
                  .BeEmpty();
            result.CommandResult
                  .Parent
                  .Children
                  .Should()
                  .ContainSingle(o => o.Name == "x");
        }

        [Fact]
        public void Arguments_only_apply_to_the_nearest_command()
        {
            var outer = new Command("outer")
            {
                new Argument<string>(),
                new Command("inner")
                {
                    new Argument<string>()
                }
            };

            ParseResult result = outer.Parse("outer inner arg1 arg2");

            result.CommandResult
                  .Parent
                  .Arguments
                  .Should()
                  .BeEmpty();
            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg1");
            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Options_only_apply_to_the_nearest_command()
        {
            var outer = new Command("outer")
                        {
                            new Command("inner")
                            {
                                new Option("-x")
                                {
                                    Argument = new Argument<string>()
                                }
                            },
                            new Option("-x")
                            {
                                Argument = new Argument<string>()
                            }
                        };

            var result = outer.Parse("outer inner -x one -x two");

            _output.WriteLine(result.ToString());

            result.RootCommandResult
                  .OptionResult("-x")
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void Subsequent_occurrences_of_tokens_matching_command_names_are_parsed_as_arguments()
        {
            var command = new Command("the-command")
            {
                new Command("complete")
                {
                    new Argument<string>(),
                    new Option("--position")
                    {
                        Argument = new Argument<int>()
                    }
                }
            };

            ParseResult result = command.Parse("the-command",
                                               "complete",
                                               "--position",
                                               "7",
                                               "the-command");

            CommandResult completeResult = result.CommandResult;

            completeResult.Arguments.Should().BeEquivalentTo("the-command");
        }

        [Fact]
        public void A_root_command_can_be_omitted_from_the_parsed_args()
        {
            var command = new Command("outer")
                          {
                              new Command("inner")
                              {
                                  new Option("-x")
                                  {
                                      Argument = new Argument
                                      {
                                          Arity = ArgumentArity.ExactlyOne
                                      }
                                  }
                              }
                          };

            var result1 = command.Parse("inner -x hello");
            var result2 = command.Parse("outer inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void A_root_command_can_match_a_full_path_to_an_executable()
        {
            var command = new RootCommand
            {
                new Command("inner")
                {
                    new Option("-x")
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ExactlyOne
                        }
                    }
                }
            };

            ParseResult result1 = command.Parse("inner -x hello");

            ParseResult result2 = command.Parse($"{RootCommand.ExePath} inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void A_renamed_RootCommand_can_be_omitted_from_the_parsed_args()
        {
            var rootCommand = new RootCommand
                              {
                                  new Command("inner")
                                  {
                                      new Option("-x")
                                      {
                                          Argument = new Argument
                                          {
                                              Arity = ArgumentArity.ExactlyOne
                                          }
                                      }
                                  }
                              };
            rootCommand.Name = "outer";

            var result1 = rootCommand.Parse("inner -x hello");
            var result2 = rootCommand.Parse("outer inner -x hello");
            var result3 = rootCommand.Parse($"{RootCommand.ExeName} inner -x hello");

            result2.RootCommandResult.Command.Should().Be(result1.RootCommandResult.Command);
            result3.RootCommandResult.Command.Should().Be(result1.RootCommandResult.Command);
        }

        [Fact]
        public void Absolute_unix_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""/temp/the file.txt""";

            var parser = new Parser(new Command("rm")
            {
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            });

            var result = parser.Parse(command);

            result.CommandResult
                  .Arguments
                  .Should()
                  .OnlyContain(a => a == @"/temp/the file.txt");
        }

        [Fact]
        public void Absolute_Windows_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""c:\temp\the file.txt\""";

            var parser = new Parser(new Command("rm")
            {
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            });

            ParseResult result = parser.Parse(command);

            result.CommandResult
                  .Arguments
                  .Should()
                  .OnlyContain(a => a == @"c:\temp\the file.txt\");
        }

        [Fact]
        public void Commands_can_have_default_argument_values()
        {
            var command = new Command("command")
            {
                new Argument<string>(() => "default")
            };

            ParseResult result = command.Parse("command");

            result.CommandResult.GetValueOrDefault().Should().Be("default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_can_still_be_accessed_as_though_it_had_been_applied()
        {
            var command = new Command("command");
            command.AddOption(
                new Option(new[] { "-o", "--option" })
                {
                    Argument = new Argument<string>(() => "the-default")
                });

            ParseResult result = command.Parse("command");

            result.HasOption("o").Should().BeTrue();
            result.HasOption("option").Should().BeTrue();
            result.ValueForOption<string>("o").Should().Be("the-default");
            result.CommandResult.ValueForOption("o").Should().Be("the-default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_result_is_implicit()
        {
            var command = new Command("command")
            {
                new Option(new[]{ "-o", "--option" })
                {
                    Argument = new Argument<string>(() => "the-default")
                }
            };

            var result = command.Parse("command");

            result.CommandResult
                  .OptionResult("o")
                  .IsImplicit
                  .Should()
                  .BeTrue();
        }

        [Fact]
        public void Command_default_argument_value_does_not_override_parsed_value()
        {
            DirectoryInfo receivedArg = null;

            var command = new Command("inner")
            {
                Argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Directory.GetCurrentDirectory()))
                {
                    Name = "arg"
                },
                Handler = CommandHandler.Create<DirectoryInfo>(arg => receivedArg = arg)
            };

            var result = command.Parse("the-directory");

            _output.WriteLine(result.ToString());

            result.CommandResult
                  .GetValueOrDefault<DirectoryInfo>()
                  .Name
                  .Should()
                  .Be("the-directory");
        }

        [Fact]
        public void Unmatched_options_are_not_split_into_smaller_tokens()
        {
            var outer = new Command("outer")
            {
                new Option("-p"),
                new Command("inner")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.OneOrMore
                    }
                }
            };

            ParseResult result = outer.Parse("outer inner -p:RandomThing=random");

            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo("-p:RandomThing=random");
        }

        [Fact]
        public void The_default_behavior_of_unmatched_tokens_resulting_in_errors_can_be_turned_off()
        {
            var command = new Command("the-command")
            {
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };
            command.TreatUnmatchedTokensAsErrors = false;

            ParseResult result = command.Parse("the-command arg1 arg2");

            result.Errors.Should().BeEmpty();

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Argument_names_can_collide_with_option_names()
        {
            IReadOnlyCollection<Symbol> symbols = new[] {
                new Option("--one")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                }};
            var command1 = new Command(
                "the-command",
                ""
            );

            foreach (var symbol in symbols)
            {
                command1.Add(symbol);
            }

            var command = command1;

            ParseResult result = command.Parse("the-command --one one");

            result.CommandResult["one"]
                  .Arguments
                  .Should()
                  .BeEquivalentTo("one");
        }

        [Fact]
        public void Option_and_Command_can_have_the_same_alias()
        {
            var innerCommand = new Command("inner")
            {
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            var option = new Option("--inner");

            var outerCommand = new Command("outer")
            {
                innerCommand,
                option,
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            var parser = new Parser(outerCommand);

            parser.Parse("outer inner").CommandResult
                  .Command
                  .Should()
                  .Be(innerCommand);

            parser.Parse("outer --inner").CommandResult
                  .Command
                  .Should()
                  .Be(outerCommand);

            parser.Parse("outer --inner inner").CommandResult
                  .Command
                  .Should()
                  .Be(innerCommand);

            parser.Parse("outer --inner inner").CommandResult
                  .Parent
                  .Children
                  .Should()
                  .Contain(c => c.Symbol == option);
        }

        [Fact]
        public void Options_can_have_the_same_alias_differentiated_only_by_prefix()
        {
            var option1 = new Option(new[] { "-a" });
            var option2 = new Option(new[] { "--a" });

            var parser = new Parser(option1, option2);

            parser.Parse("-a").CommandResult
                  .Children
                  .Select(s => s.Symbol)
                  .Should()
                  .BeEquivalentTo(option1);
            parser.Parse("--a").CommandResult
                  .Children
                  .Select(s => s.Symbol)
                  .Should()
                  .BeEquivalentTo(option2);
        }

        [Theory]
        [InlineData("-x \"hello\"", "hello")]
        [InlineData("-x=\"hello\"", "hello")]
        [InlineData("-x:\"hello\"", "hello")]
        [InlineData("-x \"\"", "")]
        [InlineData("-x=\"\"", "")]
        [InlineData("-x:\"\"", "")]
        public void When_an_argument_is_enclosed_in_double_quotes_its_value_has_the_quotes_removed(string input, string expected)
        {
            var parseResult = new Parser(
                    new Option("-x")
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }
                    })
                .Parse(input);

            parseResult["x"].Arguments
                            .Should()
                            .BeEquivalentTo(new[] { expected });
        }

        [Theory]
        [InlineData("-x -y")]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Arguments_can_start_with_prefixes_that_make_them_look_like_options(string input)
        {
            var command = new Command("command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }
                },
                new Option("-z")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }
                }
            };


            var result = command.Parse(input);

            var valueForOption = result.ValueForOption("-x");

            valueForOption.Should().Be("-y");
        }

        [Theory]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Arguments_can_match_the_aliases_of_sibling_options(string input)
        {
            var command = new Command("command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }
                },
                new Option("-y")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }
                }
            };

            var result = command.Parse(input);

            var valueForOption = result.ValueForOption("-x");

            valueForOption.Should().Be("-y");
        }

        [Theory]
        [InlineData("/o")]
        [InlineData("-o")]
        [InlineData("--o")]
        [InlineData("/output")]
        [InlineData("-output")]
        [InlineData("--output")]
        public void Option_aliases_can_be_specified_and_are_prefixed_with_defaults(string input)
        {
            var option = new Option(new[] { "output", "o" });
            var configuration = new CommandLineConfiguration(
                new[] { option },
                prefixes: new[] { "-", "--", "/" });
            var parser = new Parser(configuration);

            ParseResult parseResult = parser.Parse(input);
            parseResult["output"].Should().NotBeNull();
            parseResult["o"].Should().NotBeNull();
        }

        [Fact]
        public void Option_aliases_do_not_need_to_be_prefixed()
        {
            var option = new Option("noprefix");

            var result = new RootCommand { option }.Parse("noprefix");

            result.HasOption(option).Should().BeTrue();
        }

        [Theory]
        [InlineData("/o")]
        [InlineData("-o")]
        [InlineData("--output")]
        [InlineData("--out")]
        [InlineData("-out")]
        [InlineData("/out")]
        public void Option_aliases_can_be_specified_for_particular_prefixes(string input)
        {
            var option = new Option(new[] { "--output", "-o", "/o", "out" });
            var configuration = new CommandLineConfiguration(
                new[] { option },
                prefixes: new[] { "-", "--", "/" });
            var parser = new Parser(configuration);

            ParseResult parseResult = parser.Parse(input);
            parseResult["o"].Should().NotBeNull();
            parseResult["out"].Should().NotBeNull();
            parseResult["output"].Should().NotBeNull();
        }

        [Fact]
        public void Boolean_options_with_no_argument_specified_do_not_match_subsequent_arguments()
        {
            var command = new Command("command")
            {
                new Option("-v")
                {
                    Argument = new Argument<bool>()
                }
            };

            var result = command.Parse("-v an-argument");

            _output.WriteLine(result.ToString());

            result.ValueForOption("v").Should().Be(true);
        }

        [Fact]
        public void When_a_command_line_has_unmatched_tokens_they_are_not_applied_to_subsequent_options()
        {
            var command = new Command("command")
            {
                TreatUnmatchedTokensAsErrors = false
            };
            command.AddOption(
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                });
            command.AddOption(
                new Option("-y")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                });

            var result = command.Parse("-x 23 unmatched-token -y 42");

            result.ValueForOption("-x").Should().Be("23");
            result.ValueForOption("-y").Should().Be("42");
            result.UnmatchedTokens.Should().BeEquivalentTo("unmatched-token");
        }

        [Fact]
        public void Parse_can_be_called_with_null_args()
        {
            var parser = new Parser();

            var result = parser.Parse(null);

            result.CommandResult.Command.Name.Should().Be(RootCommand.ExeName);
        }

        [Fact]
        public void Command_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var command = new Command("the-command")
            {
                Argument = new Argument
                {
                    Arity = new ArgumentArity(3, 3)
                }
            };

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
        }

        [Fact]
        public void Command_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var command = new Command("the-command")
            {
                Argument = new Argument
                {
                    Arity = new ArgumentArity(3, 5)
                }
            };

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
            command.Parse("1 2 3 4 5")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument),
                       new Token("4", TokenType.Argument),
                       new Token("5", TokenType.Argument));
        }

        [Fact]
        public void When_command_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                Argument = new Argument
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            var result = command.Parse("1");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(ValidationMessages.Instance.RequiredArgumentMissing(result.CommandResult));
        }

        [Fact]
        public void When_command_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                Argument = new Argument
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            ParseResult parseResult = command.Parse("1 2 3 4");

            parseResult
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(ValidationMessages.Instance.UnrecognizedCommandOrArgument("4"));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = new ArgumentArity(3, 3)
                    }
                }
            };

            command.Parse("-x 1 2 3")
                   .CommandResult
                   .OptionResult("-x")
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = new ArgumentArity(3, 5)
                    }
                }
            };

            command.Parse("-x 1 2 3")
                   .CommandResult
                   .OptionResult("-x")
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
            command.Parse("-x 1 2 3 4 5")
                   .CommandResult
                   .OptionResult("-x")
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument),
                       new Token("4", TokenType.Argument),
                       new Token("5", TokenType.Argument));
        }

        [Fact]
        public void When_option_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = new ArgumentArity(2, 3)
                    }
                }
            };

            var result = command.Parse("-x 1");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(ValidationMessages.Instance.RequiredArgumentMissing(result.CommandResult.OptionResult("-x")));
        }

        [Fact]
        public void When_option_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = new ArgumentArity(2, 3)
                    }
                }
            };

            command.Parse("-x 1 2 3 4")
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(ValidationMessages.Instance.UnrecognizedCommandOrArgument("4"));
        }
    }
}
