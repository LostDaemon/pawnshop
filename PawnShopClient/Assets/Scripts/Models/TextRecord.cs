namespace PawnShop.Models
{
    public class TextRecord : IHistoryRecord
    {
        public HistoryRecordSource Source { get; }
        public string Text { get; }

        public TextRecord(HistoryRecordSource source, string text)
        {
            Source = source;
            Text = text;
        }

        public string Message => $"{Source}: {Text}";
    }
}