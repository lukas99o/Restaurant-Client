using ResturangFrontEnd.Services;

namespace Restaurant_Frontend_Tests.Helpers;

internal sealed class NoopEmailService : IEmailService
{
    public Task SendBookingConfirmationAsync(BookingConfirmationEmailModel model, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
