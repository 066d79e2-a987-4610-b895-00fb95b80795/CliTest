using System;
using System.IO;
using CliTest.Core;

namespace CliTest.Infrastructure
{
    internal class AnsiTerminal : ITerminal
    {
        private readonly Stream standardOutput;

        public AnsiTerminal() => standardOutput = Console.OpenStandardOutput();

        public void HideCursor() => EscapeSequence("?25l");

        public void MoveUp(int lines = 1)
        {
            if (lines == 0)
            {
                return;
            }
            EscapeSequence($"{lines}A");
            MoveToColumn0();
        }

        public void MoveDown(int lines = 1)
        {
            if (lines == 0)
            {
                return;
            }
            EscapeSequence($"{lines}B");
            MoveToColumn0();
        }

        private void MoveToColumn0() => EscapeSequence("0G");

        public void ClearLine() => EscapeSequence("2K");

        public void Write(string text, Style style = Style.None)
        {
            EscapeSequence($"{GetStyleCode(style)}m");
            Console.Write(text);
            EscapeSequence($"{GetStyleCode(Style.None)}m");

            string GetStyleCode(Style s) => s switch
            {
                Style.None => "0",
                Style.BoldGrey => "1;243",
                Style.BoldRed => "1;31",
                Style.BoldGreen => "1;32",
                Style.BoldYellow => "1;33",
                _ => throw new ArgumentException("Uncovered style", nameof(style))
            };
        }

        public void Flush() => standardOutput.Flush();

        private void EscapeSequence(string sequence)
        {
            standardOutput.WriteByte(0x1B);
            standardOutput.WriteByte((byte)'[');
            foreach (var c in sequence)
            {
                standardOutput.WriteByte((byte)c);
            }
        }
    }

    public static class AnsiTerminalFactory
    {
        public static ITerminal Create() => new AnsiTerminal();
    }
}
