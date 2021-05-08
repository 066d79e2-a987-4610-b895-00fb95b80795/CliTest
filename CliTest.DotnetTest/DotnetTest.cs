using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CliTest.DotnetTest
{
    public interface IDotnetTest
    {
        string Directory { get; }
        TestStatus Status { get; }
        string? Output { get; }
        Task Start();
    }

    public enum TestStatus
    {
        NotStartedYet,
        Running,
        Failed,
        Succeeded
    }

    internal class DotnetTest : IDotnetTest
    {
        public DotnetTest(string directory) => Directory = directory;

        public async Task Start()
        {
            Status = TestStatus.Running;
            var processResult = await ProcessRunner.RunProcess(
                "dotnet",
                "test",
                "--filter=Cat~Base",
                Directory);
            Status = processResult.ExitCode == 0 ? TestStatus.Succeeded : TestStatus.Failed;
            Output = processResult.Output;
        }

        public TestStatus Status { get; private set; } = TestStatus.NotStartedYet;

        public string Directory { get; }

        public string? Output { get; private set; }
    }

    public static class DotnetTestFactory
    {
        public static IDotnetTest Create(string directory) => new DotnetTest(directory);

        public static IDotnetTest[] CreateAllTestsInDirectory(string path) =>
            Directory
                .GetDirectories(path)
                .Where(d => d.ToLowerInvariant().Contains(".tests."))
                .Select(d => Create(d))
                .ToArray();
    }
}
