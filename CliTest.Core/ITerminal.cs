namespace CliTest.Core
{
    public interface ITerminal
    {
        void HideCursor();
        void MoveUp(int lines = 1);
        void MoveDown(int lines = 1);
        void ClearLine();
        void Write(string text, Style style = Style.None);
        void Flush();
    }

    public enum Style
    {
        None,
        BoldGrey,
        BoldRed,
        BoldGreen,
        BoldYellow
    }
}
