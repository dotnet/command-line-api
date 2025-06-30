// Alias workaround for https://github.com/approvals/ApprovalTests.Net/issues/768
extern alias ApprovalTests;

using ApprovalTests.ApprovalTests.Reporters;
using ApprovalTests.ApprovalTests.Reporters.TestFrameworks;

// Use globally defined Reporter for ApprovalTests. Please see
// https://github.com/approvals/ApprovalTests.Net/blob/master/docs/ApprovalTests/Reporters.md

[assembly: UseReporter(typeof(FrameworkAssertReporter))]

[assembly: ApprovalTests.ApprovalTests.Namers.UseApprovalSubdirectory("Approvals")]
