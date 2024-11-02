using DotNetEnv;
using System.Security.Cryptography;

namespace WordleBackend{
    class Program{
        public static string Generate256BitKey()
        {
            // Generate 32 bytes (256 bits) of random data
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);

            string secret = Convert.ToBase64String(key);

            // Save the generated key to the .env file
            File.AppendAllText(".env", $"\nJWT_SECRET=\"{secret}\"");

            // Convert the key to a base64 string
            return secret;
        }
        static void Main(string[] args){
            // Load environment variables from .env file
            Env.Load();

            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"serverAddress", Env.GetString("DB_SERVER_ADDRESS")},
                {"serverPort", Env.GetString("DB_SERVER_PORT")},
                {"databaseName", Env.GetString("DB_NAME")},
                {"username", Env.GetString("DB_USERNAME")},
                {"password", Env.GetString("DB_PASSWORD")},
                // If JWT_SECRET generate and store a new secret 
                {"jwtSecret", Env.GetString("JWT_SECRET") ?? Generate256BitKey()},
                {"issuer", Env.GetString("JWT_ISSUER")}
            };

            Console.WriteLine("Connecting to database...");

            WordleDB db = new WordleDB(config["serverAddress"], config["databaseName"], config["username"], config["password"], int.Parse(config["serverPort"]));

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton(db);
            builder.Services.AddSingleton(config);


            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}