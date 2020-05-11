using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(DiffReporter))]
[assembly: ApprovalTests.Namers.UseApprovalSubdirectory("Approvals")]
