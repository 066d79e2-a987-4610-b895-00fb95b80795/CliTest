using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliTest.Core;

namespace CliTest.Infrastructure
{
    internal class LocalTestHistory : ITestHistory
    {
        private string[]? failedTests;

        public bool HasPreviouslyFailed(IDotnetTest test) =>
            failedTests == null
                ? throw new InvalidOperationException($"Must call {nameof(LoadFailedTests)} before ${nameof(HasPreviouslyFailed)}")
                : failedTests.Any(t => t == test.Directory);

        public async Task LoadFailedTests()
        {
            try
            {
                using var reader = new StreamReader(FailedTestsStoragePath);
                var text = await reader.ReadToEndAsync();
                failedTests = text.Split("\n");
            }
            catch (FileNotFoundException)
            {
                failedTests = Array.Empty<string>();
            }
        }

        public async Task SaveFailedTests(IEnumerable<IDotnetTest> tests)
        {
            if (tests.Any(t => t.Status != TestStatus.Failed))
            {
                throw new ArgumentException("All tests should have failed", nameof(tests));
            }

            using var writer = new StreamWriter(FailedTestsStoragePath);
            await writer.WriteAsync(string.Join('\n', tests.Select(t => t.Directory)));
        }

        // XXX The test failures should be scoped by solution directory
        private static string FailedTestsStoragePath => $"{Environment.GetEnvironmentVariable("HOME")}/.clitest.failed";
    }

    public static class LocalTestHistoryFactory
    {
        public static ITestHistory Create() => new LocalTestHistory();
    }
}
