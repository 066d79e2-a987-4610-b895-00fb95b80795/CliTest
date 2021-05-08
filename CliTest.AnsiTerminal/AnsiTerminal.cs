using System;
using System.IO;

namespace CliTest.AnsiTerminal
{
    public interface IAnsiTerminal
    {
        void HideCursor();
        void MoveUp(int lines = 1);
        void MoveDown(int lines = 1);
        void ClearLine();
        void Write(string text, Style? style = null);
        void Flush();
    }

    internal class AnsiTerminal : IAnsiTerminal
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

        public void Write(string text, Style? style = null)
        {
            style ??= Style.None;
            EscapeSequence($"{style.Code}m");
            Console.Write(text);
            EscapeSequence($"{Style.None.Code}m");
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
        public static IAnsiTerminal CreateAnsiTerminal() => new AnsiTerminal();
    }

    public record Style
    {
        private Style(string code) => Code = code;

        internal string Code { get; }

        public static Style None => new("0");

        public static Style BoldGrey => new("1;243");

        public static Style BoldRed => new("1;31");

        public static Style BoldGreen => new("1;32");

        public static Style BoldYellow => new("1;33");
    }
}
