using Amazon.CDK;

namespace TTT.AWS.Resources
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            _ = new TableTennisTournamentStack(app, "TableTennisTournamentStack", new StackProps());
            app.Synth();
        }
    }
}
