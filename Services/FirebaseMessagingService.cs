using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Threading.Tasks;
using System.IO;

public class FirebaseMessagingService
{
    private static FirebaseApp _firebaseApp;

    public FirebaseMessagingService()
    {
        if (_firebaseApp == null)
        {
            _firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(@"D:\Copy project\BlazorApp\wwwroot\weather-alert-2dfbf-firebase-adminsdk-fbsvc-a34660a4ad.json") // ✅ Correct path
            });
        }
    }

    public async Task SendNotification(string token, string title, string body)
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("FCM Token is missing.");
            return;
        }

        var message = new Message()
        {
            Token = token,
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        try
        {
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            Console.WriteLine("Successfully sent message: " + response);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending FCM message: " + ex.Message);
        }
    }
}
