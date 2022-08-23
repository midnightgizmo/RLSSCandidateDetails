using RlssCandidateDetails.Server.Database.dbInishalize;
using RlssCandidateDetails.Server.Middleware;
using RlssCandidateDetails.Server.Models;

namespace RlssCandidateDetails.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // create a strongly typed instance of the appsettings section in appsettings.js so
            // we can access it via dependence injetion.
            //var temp = builder.Configuration.GetSection("AppSettings");
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // get an instance of the AppSettings. Normaly this will be accessed by Dependencey injection
            // but when we are in the start up code we need to access it another way
            AppSettings appSettings = app.Configuration.GetSection("AppSettings").Get<AppSettings>();

            if (app.Environment.IsDevelopment())
            {
                // check if the database exists, and if it does not create it
                Program.CreateDatabaseIfNotFound(appSettings);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            if (app.Environment.IsDevelopment())
            {
                // my own middle ware to set up cors headers ("Access-Control-Allow-Credentials" & "Access-Control-Allow-Origin")
                app.UseCorsMiddleware();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// Check if the database exists and if it does not, create it
        /// </summary>
        /// <param name="appSettings">contains the locatin of where the database should be</param>
        private static void CreateDatabaseIfNotFound(AppSettings appSettings)
        {
            dbCreator DBCreator = new dbCreator();

            if(DBCreator.DoesDataBaseExist(appSettings.DataBaseLocation) == false)
            {
                DBCreator.CreateDatabase(appSettings.DataBaseLocation);
            }
        }
    }
}