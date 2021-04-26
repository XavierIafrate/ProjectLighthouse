using System.Net.Http;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class SMSHelper
    {
        private static string API_KEY = "6C01F36C-194B-FF3B-EAC8-45A5180A4D4C";
        private static string USERNAME = "x.iafrate@wixroydgroup.com";
        private static string SENDER_NAME = "Lighthouse";

        public static async void SendText(string PhoneNumber, string Message)
        {
            using (HttpClient client = new HttpClient())
            {
                //var data = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                var response = await client.GetAsync($"https://api-mapper.clicksend.com/http/v2/send.php?method=http&username={USERNAME}&key={API_KEY}&to={PhoneNumber}&message={Message}&senderid={SENDER_NAME}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //MessageBox.Show(responseBody);
            }

        }
    }
}
