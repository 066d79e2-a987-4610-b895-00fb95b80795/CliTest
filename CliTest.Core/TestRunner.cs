using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CliTest.Core
{
    public class TestRunner
    {
        private readonly TestRenderer renderer;
        private readonly IProcessRunner processRunner;
        private readonly ITestHistory testHistory;
        private readonly ITestRunnerOptions options;

        internal TestRunner(TestRenderer renderer, IProcessRunner processRunner, ITestHistory testHistory, ITestRunnerOptions options)
        {
            this.renderer = renderer;
            this.processRunner = processRunner;
            this.testHistory = testHistory;
            this.options = options;
        }

        public async Task MainLoop()
        {
            // XXX I find it odd that this call is here. It doesn't fit here. This is not business logic, is it?
            Directory.SetCurrentDirectory(options.TargetDirectory);

            await testHistory.LoadFailedTests();

            var tests = DotnetTestFactory.CreateAllTestsInDirectory(processRunner)
                .OrderByDescending(t => testHistory.HasPreviouslyFailed(t))
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

        private async Task HandleFailedTests(IEnumerable<IDotnetTest> tests, TestRenderer renderer)
        {
            var failedTests = tests.Where(t => t.Status == TestStatus.Failed);
            await testHistory.SaveFailedTests(failedTests);

            if (!failedTests.Any())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1.3));
            renderer.WriteTestsOutput(failedTests);
        }
    }

    public interface ITestRunnerOptions
    {
        string? Filter { get; }
        int Parallelism { get; }
        string TargetDirectory { get; }
    }

    public static class TestRunnerFactory
    {
        public static TestRunner Create(ITerminal terminal, IProcessRunner processRunner, ITestHistory testHistory, ITestRunnerOptions options) =>
            new(new TestRenderer(terminal), processRunner, testHistory, options);
    }
}
