using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class EmailService(IConfiguration configuration)
{
    private readonly IConfiguration _config = configuration;




    public async Task<Result<string>> SendVerificationEmail(string toUsername, string toEmail)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Syncora", _config.GetValue<string>("EmailingConfig:Sender")));
        message.To.Add(new MailboxAddress(toUsername, toEmail));

        message.Subject = "Syncora Email Verification";

        message.Body = new TextPart("plain")
        {
            Text = "Hello! To verify your email, click the following link: https://syncora.com/verify-email"
        };

        using var client = new SmtpClient();

        try
        {
            // Connect to SMTP server
            var host = _config.GetValue<string>("EmailingConfig:Host");
            var port = _config.GetValue<int>("EmailingConfig:Port");

            client.Connect(host, port, SecureSocketOptions.StartTls);

            // Authenticate
            var username = _config.GetValue<string>("EmailingConfig:Username");
            var password = _config.GetValue<string>("EmailingConfig:Password");

            client.Authenticate(username, password);

            // Send
            await client.SendAsync(message);
            Console.WriteLine("Email sent successfully!");
            return Result<string>.Success("Email sent successfully!");

        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex.Message);
        }
        finally
        {
            // Always disconnect cleanly
            client.Disconnect(true);
        }

    }

}
