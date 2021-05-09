using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CliTest.Core
{
    public interface IDotnetTest
    {
        string Directory { get; }
        TestStatus Status { get; }
        string? Output { get; }
        Task Start(StartOptions options);
    }

    public record StartOptions(string? Filter);

    public enum TestStatus
    {
        NotStartedYet,
        Running,
        Failed,
        Succeeded
    }

    internal class DotnetTest : IDotnetTest
    {
        private readonly IProcessRunner processRunner;

        public DotnetTest(string directory, IProcessRunner processRunner)
        {
            Directory = directory;
            this.processRunner = processRunner;
        }

        public async Task Start(StartOptions options)
        {
            Status = TestStatus.Running;
            var arguments = new List<string>() { "test", Directory };
            if (options.Filter != null)
            {
                arguments.Add($"--filter={options.Filter}");
            }
            var processResult = await processRunner.RunProcess("dotnet", arguments);
            Status = processResult.ExitCode == 0 ? TestStatus.Succeeded : TestStatus.Failed;
            Output = processResult.Output;
        }

        public TestStatus Status { get; private set; } = TestStatus.NotStartedYet;

        public string Directory { get; }

        public string? Output { get; private set; }
    }

    public static class DotnetTestFactory
    {
        public static IDotnetTest Create(string directory, IProcessRunner processRunner) =>
            new DotnetTest(directory, processRunner);

        public static IDotnetTest[] CreateAllTestsInDirectory(IProcessRunner processRunner) =>
            Directory
                .GetDirectories(".")
                .Where(d => d.ToLowerInvariant().Contains(".tests.") || d.ToLowerInvariant().Contains(".test."))
                .Select(d => Create(d, processRunner))
                .ToArray();
    }
}
