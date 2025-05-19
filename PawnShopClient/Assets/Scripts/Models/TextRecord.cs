public class TextRecord : IHistoryRecord
{
    public string Source { get; }
    public string Text { get; }

    public TextRecord(string source, string text)
    {
        Source = source;
        Text = text;
    }

    public string Message => $"{Source}: {Text}";
}