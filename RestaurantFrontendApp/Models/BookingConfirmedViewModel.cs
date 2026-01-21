namespace ResturangFrontEnd.Models;

public sealed class BookingConfirmedViewModel
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? Phone { get; init; }

    public required DateTime DateLocal { get; init; }
    public required int Hour { get; init; }
    public required int Seats { get; init; }
    public required int TableId { get; init; }
}
