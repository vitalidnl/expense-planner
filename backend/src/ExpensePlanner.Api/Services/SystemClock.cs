using ExpensePlanner.Application;

namespace ExpensePlanner.Api.Services;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
