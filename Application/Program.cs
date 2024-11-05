using System.Net;
using System.Text.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Spectre.Console;

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

    public class WordHandler(){

    }

    class Program{
        static string base64Ascii= "CiAvJCQgICAgICAvJCQgICAgICAgICAgICAgICAgICAgICAgICAgICAvJCQgLyQkICAgICAgICAgIAp8ICQkICAvJCB8ICQkICAgICAgICAgICAgICAgICAgICAgICAgICB8ICQkfCAkJCAgICAgICAgICAKfCAkJCAvJCQkfCAkJCAgLyQkJCQkJCAgIC8kJCQkJCQgICAvJCQkJCQkJHwgJCQgIC8kJCQkJCQgCnwgJCQvJCQgJCQgJCQgLyQkX18gICQkIC8kJF9fICAkJCAvJCRfXyAgJCR8ICQkIC8kJF9fICAkJAp8ICQkJCRfICAkJCQkfCAkJCAgXCAkJHwgJCQgIFxfXy98ICQkICB8ICQkfCAkJHwgJCQkJCQkJCQKfCAkJCQvIFwgICQkJHwgJCQgIHwgJCR8ICQkICAgICAgfCAkJCAgfCAkJHwgJCR8ICQkX19fX18vCnwgJCQvICAgXCAgJCR8ICAkJCQkJCQvfCAkJCAgICAgIHwgICQkJCQkJCR8ICQkfCAgJCQkJCQkJAp8X18vICAgICBcX18vIFxfX19fX18vIHxfXy8gICAgICAgXF9fX19fX18vfF9fLyBcX19fX19fXy8KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAK";
        public static OSPlatform DetectOS(){
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                return OSPlatform.Windows;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                return OSPlatform.Linux;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){
                return OSPlatform.OSX;
            } else {
                throw new PlatformNotSupportedException("Unsupported OS");
            }
        }
        public static string GetConfigDir(){
            OSPlatform os = DetectOS();
            string configDir;
            if (os == OSPlatform.Windows){
                configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WordleClient");
            }
            else{
                string? path = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(path)){
                    throw new ArgumentNullException("HOME environment variable not set");
                }
                configDir = path + @"/.config/WordleClient/";
            }
            if (!Directory.Exists(configDir)){
                Directory.CreateDirectory(configDir);
            }
            return configDir;
        }
        public static JsonObject GenerateConfig(string configDir)
        {
            AnsiConsole.MarkupLine("[bold yellow]Can't find a configuration file![/]");
            AnsiConsole.MarkupLine("[bold yellow]Generating a new configuration file...[/]");
            JsonObject config = new JsonObject();
            Console.Clear();
            string AsciiDecoded = Encoding.ASCII.GetString(Convert.FromBase64String(base64Ascii));
            AnsiConsole.MarkupLine($"[bold red]{AsciiDecoded}[/]");
            AnsiConsole.MarkupLine("[bold red]Welcome to WordleClient![/]");
            
            // Prompt for the Wordle API URL
            Console.WriteLine("");
            AnsiConsole.MarkupLine("[bold]Enter the URL of the Wordle API[/]");
            AnsiConsole.Markup("[bold]Press Enter to use the default URL (https://wordle.kaktusgame.eu/api):[/]");
            string? url = Console.ReadLine();
            if (string.IsNullOrEmpty(url)){
                url = "https://wordle.kaktusgame.eu/api";
            }
            config["url"] = url;

            // Save the config to a file
            string configFile = Path.Combine(configDir, "config.json");
            File.WriteAllText(configFile, config.ToString());

            AnsiConsole.MarkupLine("[bold green]Configuration file generated![/]");

            return config;
        }

        public static JsonObject GetConfig()
        {
            string configDir = GetConfigDir();
            string configFile = Path.Combine(configDir, "config.json");

            // Check if the config file exists and is accessible
            if (!File.Exists(configFile))
            {
                return GenerateConfig(configDir);
            }

            // Read config file content safely
            string config;
            try
            {
                config = File.ReadAllText(configFile);
            }
            catch (IOException)
            {
                AnsiConsole.MarkupLine("[bold red]Error: Configuration file is currently in use by another process.[/]");
                throw;
            }

            // Check if the file content is empty or null, then regenerate the config
            if (string.IsNullOrEmpty(config))
            {
                return GenerateConfig(configDir);
            }

            var jsonNode = JsonNode.Parse(config);
            if (jsonNode == null)
            {
                return GenerateConfig(configDir);
            }
            return jsonNode.AsObject();
        }

        public static string ReadUntilValid(string question){
            string? input;
            do{
                AnsiConsole.Markup($"[bold]{question} [/]");
                input = Console.ReadLine();
            } while (string.IsNullOrEmpty(input));
            return input;
        }
        public static void PlayGame(){

        }
        
        static async Task Main(string[] args){
            Console.Clear();
            string AsciiDecoded = Encoding.ASCII.GetString(Convert.FromBase64String(base64Ascii));
            AnsiConsole.MarkupLine($"[bold red]{AsciiDecoded}[/]");
            AnsiConsole.MarkupLine("[bold red]Welcome to WordleClient![/]");
            AnsiConsole.MarkupLine("[bold]Loading configuration...[/]");
            JsonObject config = GetConfig();
            AnsiConsole.MarkupLine("[bold]Configuration loaded![/]");
            
            string url = config["url"]?.ToString() ?? throw new ArgumentNullException("URL cannot be null");
            HTTPClient client = new HTTPClient(url);

            var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select an option:")
                .AddChoices("Login", "Register"));

            Console.WriteLine("");
            Console.WriteLine("");

            string username = ReadUntilValid("Enter username:");
            string password = ReadUntilValid("Enter password:");

            if (choice == "Login"){
                string response = await HTTPClient.Login(username, password);
                AnsiConsole.MarkupLine(response);
            } else if (choice == "Register"){
                string response = await HTTPClient.Register(username, password);
                AnsiConsole.MarkupLine(response);
            }
        }
    }
}