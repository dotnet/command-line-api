using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;

// Use globally defined Reporter for ApprovalTests. Please see
// https://github.com/approvals/ApprovalTests.Net/blob/master/docs/ApprovalTests/Reporters.md

[assembly: UseReporter(typeof(FrameworkAssertReporter))]

[assembly: ApprovalTests.Namers.UseApprovalSubdirectory("Approvals")]
