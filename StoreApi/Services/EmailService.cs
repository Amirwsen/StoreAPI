using RestSharp;

public class EmailService
{
    public void SendEmail(string header, string message)
    {
        var client = new RestClient("https://send.api.mailtrap.io/api/send");
        var request = new RestRequest();
        request.AddHeader("Authorization", "Bearer 4a158b3902718be6a53e31a0f624c4b8");
        request.AddHeader("Content-Type", "application/json");
        request.AddParameter("application/json", $"{{\"from\":{{\"email\":\"hello@demomailtrap.com\",\"name\":\"Amirwsen`s Project\"}},\"to\":[{{\"email\":\"a.h.aghazadeh.1381@gmail.com\"}}],\"subject\":\"{header}\",\"text\":\"{message}\",\"category\":\"Integration Test\"}}", ParameterType.RequestBody);
        var response = client.Post(request);
        System.Console.WriteLine(response.Content);
    }
}
