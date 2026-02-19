namespace ExpensePlanner.Application;

public interface IClock
{
    DateOnly Today { get; }
}