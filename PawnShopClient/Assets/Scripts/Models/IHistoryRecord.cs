namespace PawnShop.Models
{
    public interface IHistoryRecord
    {
        public HistoryRecordSource Source { get; }
        public string Message { get; }
    }
}