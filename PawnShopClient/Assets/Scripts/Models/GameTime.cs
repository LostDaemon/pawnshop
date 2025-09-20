using System;

namespace PawnShop.Models
{
    public struct GameTime : IComparable<GameTime>, IEquatable<GameTime>
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

        public int CompareTo(GameTime other)
        {
            int thisTotalMinutes = Day * 24 * 60 + Hour * 60 + Minute;
            int otherTotalMinutes = other.Day * 24 * 60 + other.Hour * 60 + other.Minute;
            
            return thisTotalMinutes.CompareTo(otherTotalMinutes);
        }

        public bool Equals(GameTime other)
        {
            return Day == other.Day && Hour == other.Hour && Minute == other.Minute;
        }

        public override bool Equals(object obj)
        {
            return obj is GameTime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Day, Hour, Minute);
        }

        public static bool operator <(GameTime left, GameTime right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(GameTime left, GameTime right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(GameTime left, GameTime right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(GameTime left, GameTime right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator ==(GameTime left, GameTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameTime left, GameTime right)
        {
            return !left.Equals(right);
        }

        public static GameTime operator +(GameTime time, int minutes)
        {
            return time.AddMinutes(minutes);
        }

        public GameTime AddMinutes(int minutes)
        {
            int totalMinutes = (Day - 1) * 24 * 60 + Hour * 60 + Minute + minutes;
            int newDay = totalMinutes / (24 * 60) + 1; // Days start from 1
            totalMinutes %= (24 * 60);
            
            return new GameTime(newDay, totalMinutes / 60, totalMinutes % 60);
        }

        public GameTime AddHours(int hours)
        {
            return AddMinutes(hours * 60);
        }

        public GameTime AddDays(int days)
        {
            return AddMinutes(days * 24 * 60);
        }
    }
}