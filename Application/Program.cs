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
    
        public static async Task<bool> DownloadFile(string url, string path){
            try{
                using (var stream = await client.GetStreamAsync(url))
                {
                    using (var fileStream = new FileStream(path, FileMode.CreateNew))
                    {
                        await stream.CopyToAsync(fileStream);
                        return true;
                    }
                }
            }
            catch (Exception e){
                AnsiConsole.MarkupLine("[bold red]An error occurred while downloading the file.[/]");
                Console.WriteLine(e.Message);
                return false;
            }
        }
    } 
    public class ResponseDTO{
        // Example: {'h': 'green', 'e': 'yellow', 'l': 'grey', 'l': 'grey', 'o': 'yellow'}
        public required Dictionary<string, string> Response { get; set; }
    }
    public class WordHandler{
        private static List<JsonObject> historyPlay = new List<JsonObject>();

        public WordHandler(JsonObject json)
        {
            // Initialize the historyPlay list
            var historyArray = json["history"] as JsonArray;
            if (historyArray != null)
            {
                foreach (var item in historyArray)
                {
                    // Parse each string item in the history array into a JsonObject
                    if (item != null)
                    {
                        var historyItem = JsonNode.Parse(item.ToString()) as JsonObject;
                        if (historyItem != null)
                        {
                            historyPlay.Add(historyItem);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid history item format.");
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("History item cannot be null.");
                    }
                }
            }
            else
            {
                throw new ArgumentException("Invalid history data: expected a JsonArray.");
            }
        }
        public void AddAttempt(JsonObject attempt){
            historyPlay.Add(attempt);
        }
        public bool CheckForWin(JsonObject attempt){
            // Checks if there are 5 correct letters in the attempt
            var response = attempt["Response"] as JsonObject ?? attempt["response"] as JsonObject;

            if (response == null)
            {
                Console.WriteLine(attempt?.ToString());
                throw new ArgumentNullException("Response cannot be null");
            }

            foreach (var kvp in response)
            {
                if (kvp.Value?.ToString() != "green")
                {
                    return false;
                }
            }

            return true;
        }
        public string RenderPlays()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var play in historyPlay)
            {
                // Get the "Response" object from each play
                var response = play["Response"] as JsonObject ?? play["response"] as JsonObject;
                if (response == null)
                {
                    throw new ArgumentNullException("Response cannot be null");
                }

                // Iterate over each key-value pair in the response
                foreach (var kvp in response)
                {
                    // Key format is "index_letter", e.g., "0_h" or "4_o"
                    string key = kvp.Key;
                    string color = kvp.Value?.ToString() ?? throw new ArgumentNullException($"Response for key {key} cannot be null");

                    // Extract the letter from the key
                    char letter = key.Split('_')[1][0];

                    // Set the corresponding AnsiConsole color
                    if (color == "grey")
                    {
                        sb.Append("[bold grey]");
                    }
                    else if (color == "yellow")
                    {
                        sb.Append("[bold yellow]");
                    }
                    else if (color == "green")
                    {
                        sb.Append("[bold green]");
                    }

                    // Append the letter and close the markup
                    // Letter to uppercase
                    sb.Append(char.ToUpper(letter));
                    sb.Append("[/]");
                    // Add a space after each letter
                    sb.Append(" ");
                }

                // Add a newline after each word response
                sb.AppendLine();
            }

            // Output the formatted string for AnsiConsole
            return sb.ToString();
        } 
        public int GetAttempts()
        {
            return historyPlay.Count;
        }
        public string GenerateWord(string cacheDir){
            int wordCount = File.ReadLines(cacheDir + "word-bank.csv").Count();
            if (wordCount == 0){
                throw new Exception("No words in the bank.");
            }
            // Generate a random number between 0 and wordCount
            Random rn = new Random();
            // Add logic to generate a word using the random number generator
            int randomIndex = rn.Next(wordCount);
            string word = File.ReadLines(cacheDir + "word-bank.csv").Skip(randomIndex).Take(1).First();
            return word;
        }
        public static bool ValidWord(string word, string cacheDir){
            string validWords = cacheDir + "valid-words.csv";
            if (!File.ReadAllText(validWords).Contains(word)){
                return false;
            }
            return true;
        }
        public static string ValidateWord(string guessWord, string wordOfTheDay, string cacheDir){
            // Compare the guessWord with the wordOfTheDay
            // Return a ResponseDTO object
            if (guessWord.Length != 5 || !ValidWord(guessWord, cacheDir))
            {
                return "Invalid word.";
            }

            if (historyPlay.Count >= 6)
            {
                return "You have already played today.";
            }

            Dictionary<string, string> response = new Dictionary<string, string>();

            for (int i = 0; i < guessWord.Length; i++)
            {
                if (guessWord[i] == wordOfTheDay[i])
                {
                    response.Add($"{i}_{guessWord[i]}", "green");
                }
                else if (wordOfTheDay.Contains(guessWord[i]))
                {
                    response.Add($"{i}_{guessWord[i]}", "yellow");
                }
                else
                {
                    response.Add($"{i}_{guessWord[i]}", "grey");
                }
            }
            ResponseDTO responseDTO = new ResponseDTO { Response = response };
            return JsonSerializer.Serialize(responseDTO);
        }
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
        public static string GetCacheDir(){
            OSPlatform os = DetectOS();
            string cacheDir;
            if (os == OSPlatform.Windows){
                cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"WordleClient\Cache\");
            }
            else{
                string? path = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(path)){
                    throw new ArgumentNullException("HOME environment variable not set");
                }
                cacheDir = path + @"/.cache/WordleClient/";
            }
            if (!Directory.Exists(cacheDir)){
                Directory.CreateDirectory(cacheDir);
            }
            return cacheDir;
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
        public static void DownloadFiles(string cacheDir, HTTPClient HTTPClient){
            string validWords = "https://raw.githubusercontent.com/seanpatlan/wordle-words/refs/heads/main/valid-words.csv";
            string wordBank = "https://raw.githubusercontent.com/seanpatlan/wordle-words/refs/heads/main/word-bank.csv";

            bool validWordsDownloaded = HTTPClient.DownloadFile(validWords, cacheDir + "valid-words.csv").Result;
            if (!validWordsDownloaded){
                AnsiConsole.MarkupLine("[bold red]An error occurred while downloading the valid words file.[/]");
                string choice = ReadUntilValid("Do you wish to continue without the valid words file? (y/n):");
                if (choice.ToLower() != "n" && choice.ToLower() != "y"){
                    AnsiConsole.MarkupLine("[bold red]Invalid choice![/]");
                    System.Environment.Exit(1);
                }
                if (choice.ToLower() == "n"){
                    System.Environment.Exit(1);
                }
            }

            bool wordBankDownloaded = HTTPClient.DownloadFile(wordBank, cacheDir + "word-bank.csv").Result;
            if (!wordBankDownloaded){
                AnsiConsole.MarkupLine("[bold red]An error occurred while downloading the word bank file.[/]");
                string choice = ReadUntilValid("Do you wish to continue without the word bank file? (y/n):");
                if (choice.ToLower() != "n" && choice.ToLower() != "y"){
                    AnsiConsole.MarkupLine("[bold red]Invalid choice![/]");
                    System.Environment.Exit(1);
                }
                if (choice.ToLower() == "n"){
                    System.Environment.Exit(1);
                }
            }

            return;
        }
        public static bool FilesReady(string cacheDir){
            string validWords = cacheDir + "valid-words.csv";
            string wordBank = cacheDir + "word-bank.csv";

            if (!File.Exists(validWords) || !File.Exists(wordBank)){
                return false;
            }
            return true;
        }
        public static string SecretString(){
            string secret = "";
            ConsoleKeyInfo key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter){
                if (key.Key != ConsoleKey.Backspace){
                    secret += key.KeyChar;
                    Console.Write("*");
                }
                else{
                    if (secret.Length > 0){
                        secret = secret.Substring(0, secret.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                key = Console.ReadKey(true);
            }
            Console.WriteLine("");
            return secret;
        }
        public static string ReadUntilValid(string question, bool secret = false){
            string? input;
            do{
                AnsiConsole.Markup($"[bold]{question} [/]");
                if (secret){
                    input = SecretString();
                } else {
                input = Console.ReadLine();
                }
            } while (string.IsNullOrEmpty(input));
            return input;
        }
        public static async Task Login(HTTPClient HTTPClient){
            Console.Clear();
            string AsciiDecoded = Encoding.ASCII.GetString(Convert.FromBase64String(base64Ascii));
            AnsiConsole.MarkupLine($"[bold red]{AsciiDecoded}[/]");
            AnsiConsole.MarkupLine("[bold red]Welcome to login![/]");
            
            Console.WriteLine("");

            string username;
            string password;
            string response;
            do{
                username = ReadUntilValid("Enter your username:");
                password = ReadUntilValid("Enter your password:", true);
                response = await HTTPClient.Login(username, password);
                if (!response.Contains("User logged in.")){
                    if (response.Contains("Invalid username or password.")){
                        AnsiConsole.MarkupLine("[bold red]Invalid username or password.[/]");
                    } else {
                        AnsiConsole.MarkupLine("[bold red]An error occurred while logging in.[/]");
                        Console.WriteLine(response);
                        System.Environment.Exit(1);
                    }
                }
            } while (!response.Contains("User logged in."));
            AnsiConsole.MarkupLine("[bold green]User logged in successfully![/]");
            return;
        }
        public static async Task Register(HTTPClient HTTPClient){
            Console.Clear();
            string AsciiDecoded = Encoding.ASCII.GetString(Convert.FromBase64String(base64Ascii));
            AnsiConsole.MarkupLine($"[bold red]{AsciiDecoded}[/]");
            AnsiConsole.MarkupLine("[bold red]Welcome to register![/]");

            Console.WriteLine("");

            string username;
            string password;
            string response;
            do{
                username = ReadUntilValid("Enter prefered username:");
                password = ReadUntilValid("Enter prefered password:", true);
                response = await HTTPClient.Register(username, password);
                if (!response.Contains("User registered.")){
                    if (response.Contains("Username is already taken.")){
                        AnsiConsole.MarkupLine("[bold red]Username already exists.[/]");
                    } else {
                        AnsiConsole.MarkupLine("[bold red]An error occurred while registering.[/]");
                        Console.WriteLine(response);
                        System.Environment.Exit(1);
                    }
                }
            }while (!response.Contains("User registered."));

            AnsiConsole.MarkupLine("[bold green]User registered successfully![/]");
            // Wait for 3 seconds
            AnsiConsole.MarkupLine("[bold]In 3 seconds, you will be prompted to login...[/]");
            await Task.Delay(3000);
            // Login the user

            await Login(HTTPClient);
            return;
        }
        public static void Popup(string message){
            var popup = new Panel(new Markup(message))
            .Header("[bold]Popup[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse("yellow"));

            AnsiConsole.Clear();
            int centerX = 0;
            int centerY = Console.WindowHeight / 2;

            // Set cursor position to center the popup manually
            Console.SetCursorPosition(centerX, centerY);
            AnsiConsole.Write(new Align(popup, HorizontalAlignment.Center, VerticalAlignment.Middle));

            do{
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter){
            break;
            }
            } while (true);

            AnsiConsole.Clear();
            // Reset the cursor position
            Console.SetCursorPosition(0, 0);
            return;
        }
        public static async Task PlayGame(HTTPClient HTTPClient, bool offlineMode, string cacheDir = ""){
            // Check if the user has already played today
            // If they have, exit the program
            // If they haven't, continue playing the game
            JsonNode? jsonNode = null;
            string wordOfTheDay = string.Empty;
            if (!offlineMode){
                var response = await HTTPClient.Check();
                if (response.Contains("You have already played today.")){
                    AnsiConsole.MarkupLine("[bold red]You have already played today.[/]");
                    System.Environment.Exit(1);
                }
                else if (response.Contains("history")){
                    // Parse the JSON response
                    // Removes all \ from the response
                    jsonNode = JsonNode.Parse(response) ?? throw new ArgumentNullException("Parsed JSON is null");
                }
                else{
                    AnsiConsole.MarkupLine("[bold red]An error occurred while checking if you have played today.[/]");
                    Console.WriteLine(response);
                    System.Environment.Exit(1);
                }
            }
            else{
                jsonNode = JsonNode.Parse("{\"history\": []}");
            }

            if (jsonNode == null){
                throw new ArgumentNullException("Parsed JSON is null");
            }

            WordHandler wordHandler = new WordHandler(jsonNode.AsObject());
            if (offlineMode){
                wordOfTheDay = wordHandler.GenerateWord(cacheDir);
            }
            Console.Clear();

            // If font size is too small, you gonna need to enter "Babička mode" aka zoom in
            var GameLayout = new Layout("Root")
                .SplitRows(
                    new Layout("Game").Ratio(4),
                    new Layout("WordEnter")
                    .Ratio(1)
                );
            GameLayout["Game"].Update(new Panel(new Markup(wordHandler.RenderPlays(), new Style())
                .Centered())
                .Header("[bold]Game[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("red"))
                .Expand()
            );
            GameLayout["WordEnter"].Update(new Panel("Enter your guess:")
                .Header("[bold]WordEnter[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("blue"))
                .Expand()
            );
            string userInput = string.Empty;
            while (true){
                Console.CursorVisible = false;
                // Display the `WordEnter` panel with user input text dynamically
                GameLayout["WordEnter"].Update(new Panel($"Enter your guess: [blue]{userInput}[/]")
                    .Header("[bold]WordEnter[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(Style.Parse("blue"))
                    .Expand()
                );
                AnsiConsole.Clear();
                AnsiConsole.Write(GameLayout);

                // Capture single character input
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter && userInput.Length == 5)
                {
                    // Process the completed input
                    var guess = userInput.ToLower();
                    var validationResponse = string.Empty;
                    if (!offlineMode){
                        validationResponse = await HTTPClient.Validate(guess);
                    }
                    else{
                        validationResponse = WordHandler.ValidateWord(guess, wordOfTheDay, cacheDir);
                    }

                    if (validationResponse.Contains("Invalid word.")){
                        Popup("You entered an invalid word. Please try again.");
                    }
                    else if (validationResponse.Contains("You have already played today.")){
                        GameLayout["WordEnter"].Update(new Panel($"You have reached the maximum number of attempts. Good luck next time!")
                            .Header("[bold]FAILED[/]")
                            .Border(BoxBorder.Rounded)
                            .BorderStyle(Style.Parse("yellow"))
                            .Expand()
                        );
                        AnsiConsole.Write(GameLayout);
                        await Task.Delay(3000);
                        break;
                    }
                    else{
                        // Parse the JSON response
                        var json = JsonNode.Parse(validationResponse) as JsonObject;
                        if (json == null)
                        {
                            throw new ArgumentNullException("Parsed JSON is null");
                        }

                        // Add the attempt to the history
                        wordHandler.AddAttempt(json);

                        // Check if the user has won
                        if (wordHandler.CheckForWin(json))
                        {
                            GameLayout["WordEnter"].Update(new Panel($"Congratulations! Today's word was [bold]{guess.ToUpper()}[/] and you guessed it correctly in [bold]{wordHandler.GetAttempts()}[/] attempts!")
                                .Header("[bold]VICTORY[/]")
                                .Border(BoxBorder.Rounded)
                                .BorderStyle(Style.Parse("green"))
                                .Expand()
                            );
                            AnsiConsole.Write(GameLayout);
                            await Task.Delay(3000);
                            break;
                        }
                        // Update the `Game` panel with the new word history
                        GameLayout["Game"].Update(new Panel(new Markup(wordHandler.RenderPlays(), new Style())
                            .Centered())
                            .Header("[bold]Game[/]")
                            .Border(BoxBorder.Rounded)
                            .BorderStyle(Style.Parse("red"))
                            .Expand()
                        );

                        if (wordHandler.GetAttempts() >= 6)
                        {
                            GameLayout["WordEnter"].Update(new Panel($"You have reached the maximum number of attempts. Good luck next time!")
                                .Header("[bold]FAILED[/]")
                                .Border(BoxBorder.Rounded)
                                .BorderStyle(Style.Parse("yellow"))
                                .Expand()
                            );
                            AnsiConsole.Write(GameLayout);
                            await Task.Delay(3000);
                            break;
                        }

                        // Clear the user input
                        userInput = string.Empty;
                    }

                }
                else if (key.Key == ConsoleKey.Backspace && userInput.Length > 0)
                {
                    userInput = userInput.Substring(0, userInput.Length - 1);
                }
                else if (!char.IsControl(key.KeyChar) && userInput.Length < 5)
                {
                    userInput += char.ToUpper(key.KeyChar);
                }
                // Sleep for 25ms to avoid screen flickering
                await Task.Delay(50);
            }
            Console.CursorVisible = true;
            return;
        }
        
        static async Task Main(string[] args){
            Console.Clear();
            string AsciiDecoded = Encoding.ASCII.GetString(Convert.FromBase64String(base64Ascii));
            AnsiConsole.MarkupLine($"[bold red]{AsciiDecoded}[/]");
            AnsiConsole.MarkupLine("[bold red]Welcome to WordleClient![/]");
            AnsiConsole.MarkupLine("[bold]Loading configuration...[/]");
            JsonObject config = GetConfig();

            string url = config["url"]?.ToString() ?? throw new ArgumentNullException("URL cannot be null");
            HTTPClient client = new HTTPClient(url);

            AnsiConsole.MarkupLine("[bold]Configuration loaded![/]");
            AnsiConsole.MarkupLine("[bold]Checking for files required by offline mode...[/]");
            string cacheDir = GetCacheDir();
            if (!FilesReady(cacheDir)){
                AnsiConsole.MarkupLine("[bold red]Files required by offline mode are missing![/]");
                string fileChoice = ReadUntilValid("Do you wish to continue without the files -> offline mode will not be available! (y/n):");
                if (fileChoice.ToLower() != "n" && fileChoice.ToLower() != "y"){
                    AnsiConsole.MarkupLine("[bold red]Invalid choice![/]");
                    System.Environment.Exit(1);
                }
                if (fileChoice.ToLower() == "n"){
                    AnsiConsole.MarkupLine("[bold]Downloading files...[/]");
                    DownloadFiles(cacheDir, client);
                    AnsiConsole.MarkupLine("[bold]Files should be downloaded now![/]");
                }
            }
            AnsiConsole.MarkupLine("[bold]Client is ready![/]");
            Thread.Sleep(3000);

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold red]{AsciiDecoded}[/]");

            Console.WriteLine("\n\n");
            
            var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select an option:")
                .AddChoices("Login", "Register", "Offline mode"));

            Console.WriteLine("\n");

            bool offlineMode = false;

            if (choice == "Login"){
                await Login(client);
            } else if (choice == "Register"){
                await Register(client);
            }
            else if (choice == "Offline mode"){
                if (!FilesReady(cacheDir)){
                    AnsiConsole.MarkupLine("[bold red]Files required by offline mode are missing![/]");
                    System.Environment.Exit(1);
                }
                offlineMode = true;
            }
            else{
                throw new Exception($"You did something illegal and now I recieved choice: {choice}");
            }

            // Play the game
            await PlayGame(client, offlineMode, cacheDir);
            return;
        }
    }
}