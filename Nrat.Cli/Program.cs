using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nrat.AnsiTerminal;
using Nrat.DotnetTest;

namespace Nrat.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var targetDirectory = args.SingleOrDefault() ?? ".";

            Directory.SetCurrentDirectory(targetDirectory);
            var terminal = AnsiTerminalFactory.CreateAnsiTerminal();
            var renderer = new TestRenderer(terminal);
            var tests = DotnetTestFactory.StartAllTestsInDirectory(targetDirectory);
            var testExitTasks = tests.Select(t => t.ExitTask).ToList();

            foreach (var test in tests)
            {
                renderer.Draw(test);
            }

            while (testExitTasks.Count != 0)
            {
                var task = await Task.WhenAny(testExitTasks);
                var dotnetTest = await task;
                testExitTasks.Remove(task);
                renderer.Redraw(dotnetTest);
            }

            await Task.Delay(TimeSpan.FromSeconds(2));

            var failedTests = tests.Where(t => t.Status == TestStatus.Failed);
            renderer.WriteTestsOutput(failedTests);
        }
    }
}
