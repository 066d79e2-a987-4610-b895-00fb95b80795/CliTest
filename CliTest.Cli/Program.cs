using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliTest.Core;
using CliTest.Infrastructure;

namespace CliTest.Cli
{
    internal record Options
    {
        [CommandLine.Option(Required = false, HelpText = "Test filters, same as the --filter parameter for dotnet test.")]
        public string? Filter { get; set; }

        [CommandLine.Option(Required = false, HelpText = "Number of tests to run in parallel.")]
        public int Parallelism { get; set; } = 2;

        [CommandLine.Value(0, HelpText = "Directory of solution or project to run tests for.")]
        public string TargetDirectory { get; set; } = ".";
    }

    internal static class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            var parseResult = CommandLine.Parser.Default.ParseArguments<Options>(args);

            if (parseResult is CommandLine.NotParsed<Options>)
            {
                return 1;
            }

            if (parseResult is CommandLine.Parsed<Options> parsed)
            {
                var options = parsed.Value;
                await MainLoop(options);
                return 0;
            }

            throw new InvalidOperationException("Should be either parsed or not");
        }

        private static async Task MainLoop(Options options)
        {
            Directory.SetCurrentDirectory(options.TargetDirectory);
            var terminal = AnsiTerminalFactory.CreateAnsiTerminal();
            var renderer = new TestRenderer(terminal);
            var tests = DotnetTestFactory.CreateAllTestsInDirectory(ProcessRunnerFactory.Create());

            var previouslyFailedTests = await LoadPreviouslyFailedTests();
            tests = tests
                .OrderByDescending(t => previouslyFailedTests.Contains(t.Directory))
                .ThenBy(t => t.Directory)
                .ToArray();

            var testsToStart = tests.ToList();
            var testStartedTasks = new List<Task<IDotnetTest>>();

            foreach (var test in tests)
            {
                renderer.Draw(test);
            }

            while (testsToStart.Any() || testStartedTasks.Any())
            {
                while (testStartedTasks.Count < options.Parallelism && testsToStart.Any())
                {
                    var test = testsToStart.PopFirst();
                    testStartedTasks.Add(CreateTestStartedTask());
                    renderer.Redraw(test);

                    async Task<IDotnetTest> CreateTestStartedTask()
                    {
                        await test.Start(new(options.Filter));
                        return test;
                    }
                }

                var task = await Task.WhenAny(testStartedTasks);
                var dotnetTest = await task;
                _ = testStartedTasks.Remove(task);
                renderer.Redraw(dotnetTest);
            }

            await HandleFailedTests(tests, renderer);
        }

        private static async Task HandleFailedTests(IEnumerable<IDotnetTest> tests, TestRenderer renderer)
        {
            var failedTests = tests.Where(t => t.Status == TestStatus.Failed);
            await SaveFailedTests(failedTests);

            if (!failedTests.Any())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1.3));
            renderer.WriteTestsOutput(failedTests);
        }

        // XXX This save/load logic should be encapsulated in a class of its own
        // The interface should be simply bool HasPreviouslyFailed() and SaveFailedTests()
        // Do this after refactoring this to be a Main project

        private static async Task<string[]> LoadPreviouslyFailedTests()
        {
            try
            {
                using var reader = new StreamReader(FailedTestsStoragePath);
                var text = await reader.ReadToEndAsync();
                return text.Split("\n");
            }
            catch (FileNotFoundException)
            {
                return Array.Empty<string>();
            }
        }

        private static async Task SaveFailedTests(IEnumerable<IDotnetTest> failedTests)
        {
            if (failedTests.Any(t => t.Status != TestStatus.Failed))
            {
                throw new ArgumentException("All tests should have failed", nameof(failedTests));
            }

            using var writer = new StreamWriter(FailedTestsStoragePath);
            await writer.WriteAsync(string.Join('\n', failedTests.Select(t => t.Directory)));
        }

        // XXX The test failures should be scoped by solution directory
        private static string FailedTestsStoragePath => $"{Environment.GetEnvironmentVariable("HOME")}/.clitest.failed";
    }
}
