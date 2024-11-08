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
        public static string ValidateWord(string guessWord, string wordOfTheDay, string cacheDir, bool tutorial = false){
            // Compare the guessWord with the wordOfTheDay
            // Return a ResponseDTO object
            if (!tutorial){
                if (guessWord.Length != 5 || !ValidWord(guessWord, cacheDir))
                {
                    return "Invalid word.";
                }

                if (historyPlay.Count >= 6)
                {
                    return "You have already played today.";
                }
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
                    Thread.Sleep(3000);
                    System.Environment.Exit(1);
                }
                if (choice.ToLower() == "n"){
                    Thread.Sleep(3000);
                    System.Environment.Exit(1);
                }
            }

            bool wordBankDownloaded = HTTPClient.DownloadFile(wordBank, cacheDir + "word-bank.csv").Result;
            if (!wordBankDownloaded){
                AnsiConsole.MarkupLine("[bold red]An error occurred while downloading the word bank file.[/]");
                string choice = ReadUntilValid("Do you wish to continue without the word bank file? (y/n):");
                if (choice.ToLower() != "n" && choice.ToLower() != "y"){
                    AnsiConsole.MarkupLine("[bold red]Invalid choice![/]");
                    Thread.Sleep(3000);
                    System.Environment.Exit(1);
                }
                if (choice.ToLower() == "n"){
                    Thread.Sleep(3000);
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
                        Thread.Sleep(3000);
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
                        Thread.Sleep(3000);
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
                    Thread.Sleep(3000);
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
                    Thread.Sleep(3000);
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
        public static async Task Tutorial(){
            Console.Clear();
            Console.CursorVisible = false;
            string AsciiDecoded = Encoding.ASCII.GetString(Convert.FromBase64String(base64Ascii));
            AnsiConsole.MarkupLine($"[bold red]{AsciiDecoded}[/]");
            
            AnsiConsole.MarkupLine("[bold red]Welcome to the tutorial![/]");
            AnsiConsole.MarkupLine("In this tutorial, you will learn how to use this app to play Wordle!");
            AnsiConsole.MarkupLine("When you are done reading each section, press [bold]Enter to continue.[/]");
            Console.WriteLine("");  

            AnsiConsole.MarkupLine("[bold]Starting the game[/]");
            Console.WriteLine("");
            AnsiConsole.MarkupLine("After starting the app you will be greeted with a menu where you can choose to [bold]login, register, play in offline mode or read the tutorial.[/]");
            AnsiConsole.MarkupLine("You can navigate the menu by using the [bold]arrow keys[/] and pressing [bold]Enter[/] to select an option.");
            AnsiConsole.MarkupLine("By [bold]registering[/] and then [bold]logging in[/], you will be able to play the game [bold]online[/] and your progress [bold]will be saved.[/] For online mode the word is changed [bold]daily.[/]");
            AnsiConsole.MarkupLine("If you choose to play in offline mode, you will be able to play the game [bold]without an internet connection.[/] The word is [bold]randomly selected[/] from a list of words [bold]each time you play.[/]");

            var key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
            }

            AnsiConsole.Clear();

            JsonNode jsonNode = JsonNode.Parse("{\"history\": []}") ?? throw new ArgumentNullException("Parsed JSON is null");
            WordHandler wordHandler = new WordHandler(jsonNode.AsObject());
            string wordOfTheDay = "delta"; // This is reference to 1st Special Forces Operational Detachment-Delta (1st SFOD-D) aka Delta Force
            
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
            GameLayout["WordEnter"].Update(new Panel("This is game UI. In top panel you can see your previous attempts. In bottom panel you can enter your guess. Lets try it out, enter word 'HELLO'!")
                .Header("[bold]Game UI[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("yellow"))
                .Expand()
            );

            AnsiConsole.Write(GameLayout);
            key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
            }

            AnsiConsole.Clear();
            GameLayout["WordEnter"].Update(new Panel("Enter your guess:")
                .Header("[bold]WordEnter[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("blue"))
                .Expand()
            );

            string userInput = string.Empty;

            while(true){
                GameLayout["WordEnter"].Update(new Panel($"Enter your guess: [blue]{userInput}[/]")
                    .Header("[bold]WordEnter[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(Style.Parse("blue"))
                    .Expand()
                );
                AnsiConsole.Clear();
                AnsiConsole.Write(GameLayout);

                var key2 = Console.ReadKey(intercept: true);
                if (key2.Key == ConsoleKey.Enter){
                    if (userInput == "HELLO"){
                        var valid = WordHandler.ValidateWord(userInput.ToLower(), wordOfTheDay, "", true);
                        var json = JsonNode.Parse(valid) as JsonObject;
                        if (json == null)
                        {
                            throw new ArgumentNullException("Parsed JSON is null");
                        }
                        wordHandler.AddAttempt(json);
                        break;
                    }
                    else{
                        continue;
                    }
                }
                else if (key2.Key == ConsoleKey.Backspace && userInput.Length > 0){
                    userInput = userInput.Substring(0, userInput.Length - 1);
                }
                else if (!char.IsControl(key2.KeyChar) && userInput.Length < 5){
                    userInput += char.ToUpper(key2.KeyChar);
                }
                await Task.Delay(50);
            }

            AnsiConsole.Clear();
            GameLayout["Game"].Update(new Panel(new Markup(wordHandler.RenderPlays(), new Style())
                .Centered())
                .Header("[bold]Game[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("red"))
                .Expand()
            );
            GameLayout["WordEnter"].Update(new Panel("Good! Now you see that 'HELLO' poped up in the top panel. There are 3 colors: [bold]green[/], [bold]yellow[/] and [bold]grey[/]. [bold]Green[/] means that the letter is in the right place, [bold]yellow[/] means that the letter is in the word but in the wrong place and [bold]grey[/] means that the letter is not in the word.")
                .Header("[bold]Word validation[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("yellow"))
                .Expand()
            );
            AnsiConsole.Write(GameLayout);
            key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
            }

            AnsiConsole.Clear();
            GameLayout["WordEnter"].Update(new Panel("Be careful! Now second 'L' is in the word only once! But why is the first 'L' green and the second 'L' yellow? Because technically the first 'L' is in the right place and the second 'L' is in the wrong place. So if you entered only 'LLLLL' you would get 4 yellow letters and 1 green letter.")
                .Header("[bold]Word validation[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("yellow"))
                .Expand()
            );
            AnsiConsole.Write(GameLayout);
            key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
            }

            AnsiConsole.Clear();
            GameLayout["WordEnter"].Update(new Panel("Now let's enter some invalid word like 'FFFFF', shall we?")
                .Header("[bold]Word validation[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("yellow"))
                .Expand()
            );
            AnsiConsole.Write(GameLayout);
            key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
            }

            AnsiConsole.Clear();
            userInput = string.Empty;
            GameLayout["WordEnter"].Update(new Panel("Enter your guess:")
                .Header("[bold]WordEnter[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("blue"))
                .Expand()
            );
            while(true){
                GameLayout["WordEnter"].Update(new Panel($"Enter your guess: [blue]{userInput}[/]")
                    .Header("[bold]WordEnter[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(Style.Parse("blue"))
                    .Expand()
                );
                AnsiConsole.Clear();
                AnsiConsole.Write(GameLayout);

                var key2 = Console.ReadKey(intercept: true);
                if (key2.Key == ConsoleKey.Enter){
                    if (userInput == "FFFFF"){
                        Popup("Great! You triggered an error message. This is what you will see if you enter an invalid word or something else goes wrong. It's prevention for entering invalid words and loosing attempts. You can dissmiss it by pressing Enter.");
                        break;
                    }
                    else{
                        continue;
                    }
                }
                else if (key2.Key == ConsoleKey.Backspace && userInput.Length > 0){
                    userInput = userInput.Substring(0, userInput.Length - 1);
                }
                else if (!char.IsControl(key2.KeyChar) && userInput.Length < 5){
                    userInput += char.ToUpper(key2.KeyChar);
                }
                await Task.Delay(50);
            }

            AnsiConsole.Clear();
            GameLayout["WordEnter"].Update(new Panel("Great! This should be all from this, now I will tell you the word for the tutorial. The word is 'DELTA' as in 1st Special Forces Operational Detachment-Delta (1st SFOD-D) aka Delta Force. You can try to guess it now!")
                .Header("[bold]Final step[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("yellow"))
                .Expand()
            );
            AnsiConsole.Write(GameLayout);
            key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
            }

            AnsiConsole.Clear();
            userInput = string.Empty;
            GameLayout["WordEnter"].Update(new Panel("Enter your guess:")
                .Header("[bold]WordEnter[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("blue"))
                .Expand()
            );
            while(true){
                GameLayout["WordEnter"].Update(new Panel($"Enter your guess: [blue]{userInput}[/]")
                    .Header("[bold]WordEnter[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(Style.Parse("blue"))
                    .Expand()
                );
                AnsiConsole.Clear();
                AnsiConsole.Write(GameLayout);

                var key2 = Console.ReadKey(intercept: true);
                if (key2.Key == ConsoleKey.Enter){
                    if (userInput == "DELTA"){
                        var valid = WordHandler.ValidateWord(userInput.ToLower(), wordOfTheDay, "", true);
                        var json = JsonNode.Parse(valid) as JsonObject;
                        if (json == null)
                        {
                            throw new ArgumentNullException("Parsed JSON is null");
                        }
                        wordHandler.AddAttempt(json);
                        break;
                    }
                    else{
                        continue;
                    }
                }
                else if (key2.Key == ConsoleKey.Backspace && userInput.Length > 0){
                    userInput = userInput.Substring(0, userInput.Length - 1);
                }
                else if (!char.IsControl(key2.KeyChar) && userInput.Length < 5){
                    userInput += char.ToUpper(key2.KeyChar);
                }
                await Task.Delay(50);
            }

            AnsiConsole.Clear();
            GameLayout["Game"].Update(new Panel(new Markup(wordHandler.RenderPlays(), new Style())
                .Centered())
                .Header("[bold]Game[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("red"))
                .Expand()
            );
            GameLayout["WordEnter"].Update(new Panel("Congratulations! You have successfully guess to word! You have 6 attempts to guess the word. If you fail to guess the word in 6 attempts, you will be prompted with a message that you have reached the maximum number of attempts.")
                .Header("[bold]Tutorial completed[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("green"))
                .Expand()
            );
            AnsiConsole.Write(GameLayout);
            key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
            }

            AnsiConsole.Clear();
            GameLayout["WordEnter"].Update(new Panel("If you guess the word correctly, you will be prompted with a message that you have guessed the word correctly and how many attempts it took you to guess the word. This is all, now let's play!")
                .Header("[bold]Tutorial completed[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("green"))
                .Expand()
            );
            AnsiConsole.Write(GameLayout);
            key = Console.ReadKey(intercept: true);
            while (key.Key != ConsoleKey.Enter){
                key = Console.ReadKey(intercept: true);
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
                    Thread.Sleep(3000);
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
                .AddChoices("Login", "Register", "Offline mode", "Tutorial"));

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
                    Thread.Sleep(3000);
                    System.Environment.Exit(1);
                }
                offlineMode = true;
            }
            else if (choice == "Tutorial"){
                await Tutorial();
                System.Environment.Exit(1);
            }
            else{
                throw new Exception($"You did something illegal and now I recieved choice: {choice}");
            }

            // Play the game
            await PlayGame(client, offlineMode, cacheDir);
            Thread.Sleep(3000);
            return;
        }
    }
}