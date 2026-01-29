using System;
using DuelApp.Shared.Abstractions.Time;

namespace DuelApp.Shared.Infrastructure.Time
{
    internal class UtcClock : IClock
    {
        public DateTime CurrentDate() => DateTime.UtcNow;
    }
}