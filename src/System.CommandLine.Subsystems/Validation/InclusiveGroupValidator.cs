// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.ValueConditions;

namespace System.CommandLine.Validation;

/// <summary>
/// Validator that requires that if one member of the group is present, they are all present.
/// </summary>
public class InclusiveGroupValidator : CommandValidator
{
    public InclusiveGroupValidator() : base(nameof(InclusiveGroup), typeof(InclusiveGroup))
    { }

    /// <inheritdoc/>
    public override void Validate(CliCommandResult commandResult,
        CommandCondition valueCondition, ValidationContext validationContext)
    {
        var commandSymbol = commandResult.Command;
        // TODO: Write the SymbolsInUse method. I think this should allow for default values, so it requires some thought. Hopefully ValueResult already returns only those vaues that the user entered. 
        var symbolsInUse = commandResult.ValueResults.Select(x => x.ValueSymbol); // commandResult.SymbolsInUse();
        var inclusiveGroup = GetTypedValueConditionOrThrow<InclusiveGroup>(valueCondition);
        var groupMembers = inclusiveGroup.Members;
        var groupInUse = groupMembers
                        .Any(x => symbolsInUse.Contains(x));
        if (!groupInUse)
        {
            return;
        }
        // TODO: Lazily create the missing member list
        // TODO: See if there is a LINQ set method for "all not in the other list"
        var missingMembers = new List<CliValueSymbol>();
        foreach (var member in groupMembers)
        {
            if (!symbolsInUse.Contains(member))
            {
                missingMembers.Add(member);
            }
        }
        if (missingMembers is not null && missingMembers.Any())
        {
            // TODO: Rework to allow localization
            var pluralToBe = "are";
            var singularToBe = "is";
            validationContext.AddError(new CliDiagnostic(new("", "", "The members {groupMembers} " +
            "must all be used if one is used. {missingMembers} " +
            "{toBe} missing.", severity: CliDiagnosticSeverity.Error, null), [string.Join(", ", groupMembers.Select(m => m.Name)), string.Join(", ", missingMembers.Select(m => m.Name)), (missingMembers.Skip(1).Any() ? pluralToBe : singularToBe)]));
        }
    }
}
