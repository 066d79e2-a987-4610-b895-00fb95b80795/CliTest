using CliTest.Core;

namespace CliTest.Infrastructure
{
    public static class CliParser
    {
        public static ITestRunnerOptions? Parse(string[] args)
        {
            var parseResult = CommandLine.Parser.Default.ParseArguments<CliOptions>(args);
            return parseResult switch
            {
                CommandLine.Parsed<CliOptions> parsed => parsed.Value,
                _ => null
            };
        }
    }

    internal record CliOptions : ITestRunnerOptions
    {
        [CommandLine.Option(Required = false, HelpText = "Test filters, same as the --filter parameter for dotnet test.")]
        public string? Filter { get; set; }

        [CommandLine.Option(Required = false, HelpText = "Number of tests to run in parallel.")]
        public int Parallelism { get; set; } = 2;

        [CommandLine.Value(0, HelpText = "Directory of solution or project to run tests for.")]
        public string TargetDirectory { get; set; } = ".";
    }
}
