using PostmarkDotNet;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _postmarkApiKey = "0184de91-0392-4500-a671-93b2e296133e"; // <-- Hardcoded API Key
    private readonly string _fromEmail = "vikram.e.cst.2022@snsce.ac.in"; // <-- Hardcoded Sender Email

    public EmailService() { } // No need for IConfiguration

    public async Task SendWeatherAlertEmailAsync(string toEmail, string city, string alertHeadline, string alertDescription)
    {
        var client = new PostmarkClient(_postmarkApiKey);

        var message = new PostmarkMessage
        {
            From = _fromEmail,
            To = toEmail,
            Subject = $"🚨 Weather Alert for {city}",
            HtmlBody = $"<h3>🚨 Weather Alert for {city}</h3><p><strong>{alertHeadline}</strong></p><p>{alertDescription}</p>",
            TextBody = $"Weather Alert for {city}:\n{alertHeadline}\n{alertDescription}"
        };

        var response = await client.SendMessageAsync(message);

        if (response.Status != PostmarkStatus.Success)
        {
            Console.WriteLine($"Error sending email: {response.Message}");
        }
    }
}
