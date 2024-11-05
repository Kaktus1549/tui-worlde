using System.Net;
using System.Text.Json;
using System.Text;

namespace WordleClient{
    public class HTTPClient{
        private static readonly HttpClientHandler handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
        private static readonly HttpClient client = new HttpClient(handler);
        private static string? url;

        public HTTPClient(string _url){
            if (string.IsNullOrEmpty(_url)){
                throw new ArgumentNullException("URL cannot be null or empty");
            }
            url = _url;
        }
        
        public static async Task<string> Login(string username, string password){
            var jsonPayload = new
            {
                username = username,
                password = password
            };

            // Convert the payload to JSON
            var jsonContent = new StringContent(JsonSerializer.Serialize(jsonPayload), Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(url + "/login", jsonContent);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> Register(string username, string password){
            var jsonPayload = new
            {
                username = username,
                password = password
            };

            // Convert the payload to JSON
            var jsonContent = new StringContent(JsonSerializer.Serialize(jsonPayload), Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(url + "/register", jsonContent);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetStats(){
            var response = await client.GetAsync(url + "/stats");
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> Check(){
            var response = await client.GetAsync(url + "/check");
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> Validate(string guess){
            var jsonPayload = new
            {
                Word = guess
            };

            // Convert the payload to JSON
            var jsonContent = new StringContent(JsonSerializer.Serialize(jsonPayload), Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(url + "/validate", jsonContent);
            return await response.Content.ReadAsStringAsync();
        }
    } 

    class Program{
        static async Task Main(string[] args){
            var client = new HTTPClient("https://wordle.kaktusgame.eu/api");
            Console.WriteLine(await HTTPClient.Login("username", "password"));
            Console.WriteLine(await HTTPClient.GetStats());
            Console.WriteLine(await HTTPClient.Check());
            Console.WriteLine(await HTTPClient.Validate("hello"));
            Console.WriteLine(await HTTPClient.Check());
        }
    }
}