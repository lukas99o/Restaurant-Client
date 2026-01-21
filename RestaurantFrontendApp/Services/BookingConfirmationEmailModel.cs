namespace ResturangFrontEnd.Services;

public sealed class BookingConfirmationEmailModel
{
    public required string ToEmail { get; init; }
    public required string Name { get; init; }

    public required DateTime DateLocal { get; init; }
    public required int Hour { get; init; }
    public required int Seats { get; init; }
    public required int TableId { get; init; }

    public string? Phone { get; init; }
}
