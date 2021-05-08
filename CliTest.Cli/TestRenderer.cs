using System;
using System.Collections.Generic;
using CliTest.AnsiTerminal;
using CliTest.DotnetTest;

namespace CliTest.Cli
{
    public class TestRenderer
    {
        private readonly IAnsiTerminal terminal;
        private readonly Dictionary<IDotnetTest, int> testToLine = new();

        public TestRenderer(IAnsiTerminal terminal) => this.terminal = terminal;

        public void Draw(IDotnetTest test)
        {
            terminal.MoveDown(testToLine.Count);
            testToLine.Add(test, testToLine.Count);
            DrawTestOnCurrentLine(test);
            terminal.Write("\n");
            terminal.MoveUp(testToLine.Count);
        }

        public void Redraw(IDotnetTest test)
        {
            var lineCount = testToLine[test];
            terminal.MoveDown(lineCount);
            DrawTestOnCurrentLine(test);
            terminal.MoveUp(lineCount);
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
                    _ => throw new ArgumentException(nameof(test))
                };

            static Style GetStatusStyle(IDotnetTest test) =>
                test.Status switch
                {
                    TestStatus.NotStartedYet => Style.BoldGrey,
                    TestStatus.Running => Style.BoldYellow,
                    TestStatus.Failed => Style.BoldRed,
                    TestStatus.Succeeded => Style.BoldGreen,
                    _ => throw new ArgumentException(nameof(test))
                };
        }
    }
}
