using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliTest.AnsiTerminal;
using CliTest.Core;

namespace CliTest.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var targetDirectory = args.SingleOrDefault() ?? ".";

            var parallelism = 2;

            Directory.SetCurrentDirectory(targetDirectory);
            var terminal = AnsiTerminalFactory.CreateAnsiTerminal();
            var renderer = new TestRenderer(terminal);
            var tests = DotnetTestFactory.CreateAllTestsInDirectory(targetDirectory);
            var testsToStart = tests.ToList();
            var testStartedTasks = new List<Task<IDotnetTest>>();

            foreach (var test in tests)
            {
                renderer.Draw(test);
            }

            while (testsToStart.Any() || testStartedTasks.Any())
            {
                while (testStartedTasks.Count < parallelism)
                {
                    var test = testsToStart.PopFirst();
                    testStartedTasks.Add(CreateTestStartedTask());
                    renderer.Redraw(test);

                    async Task<IDotnetTest> CreateTestStartedTask()
                    {
                        await test.Start();
                        return test;
                    }

                    if (testStartedTasks.Count == 0)
                    {
                        break;
                    }
                }

                var task = await Task.WhenAny(testStartedTasks);
                var dotnetTest = await task;
                testStartedTasks.Remove(task);
                renderer.Redraw(dotnetTest);
            }

            await Task.Delay(TimeSpan.FromSeconds(2));

            var failedTests = tests.Where(t => t.Status == TestStatus.Failed);
            renderer.WriteTestsOutput(failedTests);
        }
    }
}
