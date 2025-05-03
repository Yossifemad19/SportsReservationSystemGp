

using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using backend.Api.Services.Interfaces;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public SendGridEmailService(IConfiguration config)
    {
        _apiKey = config["SendGrid:ApiKey"];
        _senderEmail = config["SendGrid:SenderEmail"];
        _senderName = config["SendGrid:SenderName"];
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_senderEmail, _senderName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, message, $"<p>{message}</p>");
        await client.SendEmailAsync(msg);
    }
}
