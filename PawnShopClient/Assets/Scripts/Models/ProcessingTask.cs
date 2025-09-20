namespace PawnShop.Models
{
    public class ProcessingTask
    {
        public ProcessingType ProcessingType { get; set; }
        public ItemModel Item { get; set; }
        public GameTime ReadyAt { get; set; }
    }
}
