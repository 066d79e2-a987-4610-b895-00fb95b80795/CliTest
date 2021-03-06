using System;
using System.Collections.Generic;

namespace CliTest.Core
{
    internal class TestRenderer
    {
        private readonly ITerminal terminal;
        private readonly Dictionary<IDotnetTest, int> testToLine = new();

        public TestRenderer(ITerminal terminal) => this.terminal = terminal;

        public void Draw(IDotnetTest test)
        {
            testToLine.Add(test, testToLine.Count);
            DrawTestOnCurrentLine(test);
            terminal.Write("\n");
        }

        public void Redraw(IDotnetTest test)
        {
            var lineCount = testToLine.Count - testToLine[test];
            terminal.MoveUp(lineCount);
            DrawTestOnCurrentLine(test);
            terminal.MoveDown(lineCount);
        }

        public void WriteTestsOutput(IEnumerable<IDotnetTest> tests)
        {
            terminal.MoveDown(testToLine.Count);
            foreach (var test in tests)
            {
                terminal.Write(test.Output ?? throw new InvalidOperationException($"Output of {test.Directory} test is null"));
                terminal.Write("\n");
            }
            terminal.Flush();
        }

        private void DrawTestOnCurrentLine(IDotnetTest test)
        {
            terminal.ClearLine();
            terminal.Write($"{test.Directory} — ");
            terminal.Write(GetStatusString(test), GetStatusStyle(test));

            static string GetStatusString(IDotnetTest test) =>
                test.Status switch
                {
                    TestStatus.NotStartedYet => "waiting",
                    TestStatus.Running => "running",
                    TestStatus.Failed => "failed",
                    TestStatus.Succeeded => "succeeded",
                    _ => throw new ArgumentException("Invalid test status", nameof(test))
                };

            static Style GetStatusStyle(IDotnetTest test) =>
                test.Status switch
                {
                    TestStatus.NotStartedYet => Style.BoldGrey,
                    TestStatus.Running => Style.BoldYellow,
                    TestStatus.Failed => Style.BoldRed,
                    TestStatus.Succeeded => Style.BoldGreen,
                    _ => throw new ArgumentException("Invalid test status", nameof(test))
                };
        }
    }
}
