// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", 
    "CA2016: Forward the 'CancellationToken' parameter to methods that take one", 
    Justification = "Test is designed to illustrate behaviour with abandonment when cancellation token is not observed.")]
