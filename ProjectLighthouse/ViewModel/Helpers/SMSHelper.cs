using System.Net.Http;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class SMSHelper
    {
        private static string API_KEY;
        private static string USERNAME = "x.iafrate@wixroydgroup.com";
        private static string SENDER_NAME = "Lighthouse";

        public static async void SendText(string PhoneNumber, string Message)
        {
            API_KEY = ClickSendCreds.API_KEY;
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync($"https://api-mapper.clicksend.com/http/v2/send.php?method=http&username={USERNAME}&key={API_KEY}&to={PhoneNumber}&message={Message}&senderid={SENDER_NAME}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

        }
    }
}
