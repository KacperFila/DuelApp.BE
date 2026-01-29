using System;

namespace DuelApp.Shared.Abstractions.Time
{
    public interface IClock
    {
        DateTime CurrentDate();
    }
}