using System.CommandLine.Rendering.Views;

namespace System.CommandLine.Rendering.Help
{
    public class SynopsisView : ContentView<ICommand>
    {
        public SynopsisView(ICommand value) : base(value)
        {
        }

        protected override Span CreateSpan(ConsoleRenderer renderer)
        {
            //var title = $"{command.Name}:";
            //HelpSection.Write(this, title, command.Description);
            var span = renderer.Formatter.ParseToSpan($"{Value.Name} {Value.Description}");
            return span;
        }
    }
}
