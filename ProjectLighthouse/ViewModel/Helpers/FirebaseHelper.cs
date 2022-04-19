
using ProjectLighthouse.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class FirebaseHelper
    {

        private static readonly string dbPath = "https://lighthouse-a3719-default-rtdb.europe-west1.firebasedatabase.app/";
        private static readonly string KEY = "ffrvPRafMG24y9Zxlzuj9lFhGlgOIywfFi0oOsdR";

        public static async void SynchroniseSchedule(List<LatheManufactureOrder> allOrders)
        {
            List<LatheManufactureOrder> onSchedule = allOrders.Where(x => x.State < OrderState.Complete && !string.IsNullOrEmpty(x.AllocatedMachine)).ToList();
            List<LatheManufactureOrder> ordersOnFirebase = await Read<LatheManufactureOrder>();
            for (int i = 0; i < ordersOnFirebase.Count; i++)
            {
                if (!onSchedule.Contains(ordersOnFirebase[i]))
                {
                    _ = await Delete(ordersOnFirebase[i]);
                    LatheManufactureOrder masterOrder = allOrders.Find(x => x.Name == ordersOnFirebase[i].Name);
                    masterOrder.FirebaseId = null;
                    DatabaseHelper.Update(masterOrder);
                }
            }

            PostSchedule(onSchedule);
        }

        public static async void PostSchedule(List<LatheManufactureOrder> orders)
        {
            for (int i = 0; i < orders.Count; i++)
            {
                if (string.IsNullOrEmpty(orders[i].FirebaseId))
                {
                    string newId= await Insert(orders[i]);
                    Debug.WriteLine($"HTTP SUCCESS: [{newId}]");
                    orders[i].FirebaseId=newId;
                    DatabaseHelper.Update(orders[i]);
                }
                else
                {
                    await Update(orders[i]);
                }
            }
        }

        public static async Task<string> Insert<T>(T item)
        {
            if (item == null)
            {
                return null;
            }

            string jsonBody = JsonSerializer.Serialize(item);
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

            using HttpClient client = new();
            HttpResponseMessage result = await client.PostAsync($"{dbPath}{item.GetType().Name.ToLower()}.json?auth={KEY}", content);
            
            if (result.IsSuccessStatusCode)
            {
                string jsonResult = await result.Content.ReadAsStringAsync();
                FirebaseKeyResponse newKey = Newtonsoft.Json.JsonConvert.DeserializeObject<FirebaseKeyResponse>(jsonResult);
                return newKey.Name;
            }
            else
            {
                Debug.WriteLine(result.ReasonPhrase);
                return null;
            }
        }

        public static async Task<bool> Update<T>(T item) where T : ISchedulableObject
        {
            string jsonBody = JsonSerializer.Serialize(item);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using HttpClient client = new();
            var result = await client.PatchAsync($"{dbPath}{item.GetType().Name.ToLower()}/{item.FirebaseId}.json", content);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> Delete<T>(T item) where T : ISchedulableObject
        {
            using HttpClient client = new();
            var result = await client.DeleteAsync($"{dbPath}{item.GetType().Name.ToLower()}/{item.FirebaseId}.json");

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<List<T>?> Read<T>() where T : ISchedulableObject
        {
            using HttpClient client = new();
            var result = await client.GetAsync($"{dbPath}{typeof(T).Name.ToLower()}.json");
            var jsonResult = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
            {
                var objects = JsonSerializer.Deserialize<Dictionary<string, T>>(jsonResult);
                List<T> list = new();

                if (objects != null)
                {
                    foreach (var o in objects)
                    {
                        o.Value.FirebaseId = o.Key;
                        list.Add(o.Value);
                    }
                }


                return list;
            }
            else
            {
                return null;
            }
        }

        private class FirebaseKeyResponse
        {
            public string Name { get; set; }
        }
    }
}
