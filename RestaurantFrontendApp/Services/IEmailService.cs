namespace ResturangFrontEnd.Services;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(BookingConfirmationEmailModel model, CancellationToken cancellationToken = default);
}
