using RlssCandidateDetails.Server.Middleware;
using RlssCandidateDetails.Server.Models;
using System.Runtime.CompilerServices;

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

            // creates a timer that periodicly removes Refresh tokens from the database that have expired

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            // every 5 mins run a database query to remove all expired refresh tokens (this runs in another thread).
            // When this app finishes we will call the CancellationTokenSource.Cancel() to tell the thread created
            // in the below function to exit.
            ConfigureRefreshTokenExpiryWatcher(appSettings, cancellationTokenSource.Token);

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

            // my own middleware to respond to Pre Flight requests.
            // neded for when the client wants to send back Json Web Tokens, It sends a pre flight
            // request asking if the authorization header is allowed.
            app.UsePreFlightRequestMiddleware();

            // Custom Authentication Middle wear that determins if a user is logged in (authenticated)
            // checks to see if the JWT is pressent and if so sets the users roles based on the roles value in the JWT
            app.UseAuthenticationMiddleware();
            app.UseAuthorization();


            app.MapControllers();

            

             app.Run();


            cancellationTokenSource.Cancel();

            // need a way to cancel the thread (thread.abort not supported any more), need to use cancelation tokens
            // http://classport.blogspot.com/2014/05/cancellationtoken-and-threadsleep.html
            //if (_RefreshTokenThread != null)
            //    _RefreshTokenThread
            
        }

        private static Thread _RefreshTokenThread;
        /// <summary>
        /// Creates a timer that periodicly removes Refresh tokens from the database that have expired
        /// </summary>
        /// <param name="appSettings">Contains the location of the Refresh Token Database</param>
        /// <param name="Token">Token the thread will check to see when the thread should exit</param>
        /// <exception cref="NotImplementedException">Containers the database location for the refresh token database</exception>
        private static void ConfigureRefreshTokenExpiryWatcher(AppSettings appSettings, CancellationToken Token)
        {
            ParameterizedThreadStart ThreadMethod;
            
            
            // create an anonymouse method that will delete expired refresh tokens every 5 mins.
            // This will get called by another thread
            ThreadMethod = delegate (object DataBaseLocation)
            {
                // set a time span of 10 mins
                TimeSpan SleepTime = new TimeSpan(0, 10, 0);
                // keep looping every 5 mins or until the Token has been cancelled
                while(!Token.IsCancellationRequested)
                {
                    // remove all expired refresh tokens from the database
                    RefreshToken.TokenManager.RemoveExpiredTokens((string)DataBaseLocation);
                    // sleep for 5 mins unless the CcncellationToken has been signalled to cancel
                    Token.WaitHandle.WaitOne(SleepTime);
                    //System.Threading.Thread.Sleep(SleepTime);
                }
            };

            // Create a new thread that will call the above ThreadMethod
            _RefreshTokenThread = new Thread(ThreadMethod);
            // make sure thread is background so it gets destryoed when the main thread finishes (program exits)
            _RefreshTokenThread.IsBackground = true;
            // start the thread passing in the Refresh Token database location
            _RefreshTokenThread.Start(appSettings.TokenManagerDatabaseLocation);
        }

        /// <summary>
        /// Check if the database exists and if it does not, create it
        /// </summary>
        /// <param name="appSettings">contains the locatin of where the database should be</param>
        private static void CreateDatabaseIfNotFound(AppSettings appSettings)
        {
            // create the main database
            RlssCandidateDetails.Server.Database.dbInishalize.dbCreator DBCreator = new RlssCandidateDetails.Server.Database.dbInishalize.dbCreator();

            if(DBCreator.DoesDataBaseExist(appSettings.DataBaseLocation) == false)
            {
                DBCreator.CreateDatabase(appSettings.DataBaseLocation);
            }

            // Create the Refresh token database
            RlssCandidateDetails.RefreshToken.Database.dbInishalize.dbCreator refreshTokenDB = new RefreshToken.Database.dbInishalize.dbCreator();

            if(refreshTokenDB.DoesDataBaseExist(appSettings.TokenManagerDatabaseLocation) == false)
            {
                refreshTokenDB.CreateDatabase(appSettings.TokenManagerDatabaseLocation);
            }



        }
    }
}