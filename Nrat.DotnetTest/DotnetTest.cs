using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nrat.DotnetTest
{
    public interface IDotnetTest
    {
        string Directory { get; }
        TestStatus Status { get; }
        string? Output { get; }
        Task<IDotnetTest> ExitTask { get; }
    }

    public enum TestStatus
    {
        Running,
        Failed,
        Succeeded
    }

    internal class DotnetTest : IDotnetTest
    {
        private DotnetTest(string directory, Task<ProcessResult> processTask)
        {
            Directory = directory;
            ExitTask = CreateExitTask();

            async Task<IDotnetTest> CreateExitTask()
            {
                var processResult = await processTask;
                Status = processResult.ExitCode == 0 ? TestStatus.Succeeded : TestStatus.Failed;
                Output = processResult.Output;
                return this;
            }
        }

        public static DotnetTest Start(string directory)
        {
            var task = ProcessRunner.RunProcess(
                "dotnet",
                "test",
                "--filter=Cat~Base",
                directory);
            return new(directory, task);
        }

        public TestStatus Status { get; private set; } = TestStatus.Running;

        public string Directory { get; }

        public string? Output { get; private set; }

        public Task<IDotnetTest> ExitTask { get; }
    }

    public static class DotnetTestFactory
    {
        public static IDotnetTest StartDotnetTest(string directory) =>
            DotnetTest.Start(directory);

        public static IDotnetTest[] StartAllTestsInDirectory(string path) =>
            Directory
                .GetDirectories(path)
                .Where(d => d.ToLowerInvariant().Contains(".tests."))
                .Select(d => StartDotnetTest(d))
                .ToArray();
    }
}
