namespace PawnShop.Models
{
    public struct GameTime
    {
        public int Day;
        public int Hour;
        public int Minute;

        public GameTime(int day, int hour, int minute)
        {
            Day = day;
            Hour = hour;
            Minute = minute;
        }

        public override string ToString()
        {
            return $"Day {Day} {Hour:00}:{Minute:00}";
        }
    }
}