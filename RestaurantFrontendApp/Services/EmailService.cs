using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;

namespace ResturangFrontEnd.Services;

public sealed class EmailService(IOptions<EmailOptions> options) : IEmailService
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendBookingConfirmationAsync(BookingConfirmationEmailModel model, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        var fromName = string.IsNullOrWhiteSpace(_options.FromName) ? _options.FromAddress : _options.FromName;
        message.From.Add(new MailboxAddress(fromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(model.ToEmail));
        message.Subject = "Booking confirmed - Restaurant Kifo";

        var date = model.DateLocal.ToString("yyyy-MM-dd");
        var timeWindow = $"{model.Hour:00}:00 - {model.Hour + 1:00}:00";

        var safeName = System.Net.WebUtility.HtmlEncode(model.Name);
        var safeEmail = System.Net.WebUtility.HtmlEncode(model.ToEmail);
        var phoneLine = string.IsNullOrWhiteSpace(model.Phone)
            ? ""
            : $"<div><strong>Phone:</strong> {System.Net.WebUtility.HtmlEncode(model.Phone)}</div>";

        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html>");
        sb.AppendLine("  <body style=\"font-family:Arial,Helvetica,sans-serif; line-height:1.4; color:#111;\">");
        sb.AppendLine("    <h2 style=\"margin:0 0 12px 0;\">Your booking is confirmed</h2>");
        sb.AppendLine($"    <p style=\"margin:0 0 12px 0;\">Hi {safeName},</p>");
        sb.AppendLine("    <p style=\"margin:0 0 12px 0;\">We have successfully registered your booking at <strong>Restaurant Kifo</strong>.</p>");
        sb.AppendLine("    <div style=\"border:1px solid #e5e5e5; padding:12px; border-radius:8px;\">");
        sb.AppendLine($"      <div><strong>Date:</strong> {date}</div>");
        sb.AppendLine($"      <div><strong>Time:</strong> {timeWindow}</div>");
        sb.AppendLine($"      <div><strong>Seats:</strong> {model.Seats}</div>");
        sb.AppendLine($"      <div><strong>Table:</strong> {model.TableId}</div>");
        if (!string.IsNullOrWhiteSpace(phoneLine))
        {
            sb.AppendLine($"      {phoneLine}");
        }
        sb.AppendLine($"      <div><strong>Email:</strong> {safeEmail}</div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <p style=\"margin:12px 0 0 0;\">If you need to change or cancel your booking, please contact us.</p>");
        sb.AppendLine("    <p style=\"margin:12px 0 0 0; color:#666;\">This is an automated message, please do not reply.</p>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");

        message.Body = new BodyBuilder { HtmlBody = sb.ToString() }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await smtp.AuthenticateAsync(_options.SmtpUser, _options.SmtpPass, cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}
