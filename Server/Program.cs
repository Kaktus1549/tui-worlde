using DotNetEnv;

namespace WordleBackend{
    class Program{
        static void Main(string[] args){
            // Load environment variables from .env file
            Env.Load();

            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"serverAddress", Env.GetString("DB_SERVER_ADDRESS")},
                {"databaseName", Env.GetString("DB_NAME")},
                {"username", Env.GetString("DB_USERNAME")},
                {"password", Env.GetString("DB_PASSWORD")}
            };

            WordleDB db = new WordleDB(config["serverAddress"], config["databaseName"], config["username"], config["password"]);

            var builder = WebApplication.CreateBuilder(args);

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

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}