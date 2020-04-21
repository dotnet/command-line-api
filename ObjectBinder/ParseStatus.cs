namespace J4JSoftware.CommandLine
{
    public class ParseStatus
    {
        public bool HelpRequested { get; set; }
        public bool FoundErrors { get; set; }
        public bool IsValid => !HelpRequested && !FoundErrors;
    }
}