namespace lesson58.Services;

public class EmailService
{

    public async Task SendEmail(string email, string subject, string text)
    {
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

        string server = "smtp.mail.ru";
        int port = 25;
        bool enableSsl = false;
        string from = "imanbekovtologon4@mail.ru";
        string password = "zqaBX9jnQcvNh4cNnm8p";
        string to = email;

        var message = new MimeKit.MimeMessage();
        message.From.Add(new MimeKit.MailboxAddress("тологон", from));
        message.To.Add(new MimeKit.MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new MimeKit.TextPart("plain")
        {
            Text = text
        };

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            await client.ConnectAsync(server, port, enableSsl);
            await client.AuthenticateAsync(from, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
    
}