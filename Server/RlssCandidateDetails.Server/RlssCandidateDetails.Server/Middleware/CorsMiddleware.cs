

namespace RlssCandidateDetails.Server.Middleware
{
    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;


        public CorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }



        public async Task Invoke(HttpContext httpContext)
        {

            // sets the cors headers ("Access-Control-Allow-Credentials" & "Access-Control-Allow-Origin")


            string url;
            // get the clients url
            Uri clientUri = httpContext.Request.GetTypedHeaders().Referer;
            // if we could not find the url, we can't set up cors
            if (clientUri == null)
            {
                // move onto the next middleware
                await _next(httpContext);
                return;
            }

            // create the url using the scheme, the domainname we have set in the appsettings.json file and the port number if the client has one set
            //url = clientUri.Scheme + "://" + appSettings.Value.DomainName;
            url = clientUri.Scheme + "://" + "localhost";
            // check to see if a port has been attached to the uri
            string[] splitURL = clientUri.OriginalString.Split("://");
            int portPosition = splitURL[splitURL.Length - 1].LastIndexOf(':');
            if (portPosition != -1)
            {
                // add the port to the url
                string sPortNumber = splitURL[splitURL.Length - 1].Substring(portPosition).Replace("/", "");
                url += sPortNumber;
            }

            // set the headers for communicating between differnet origins (CORS)
            httpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            httpContext.Response.Headers.Add("Access-Control-Allow-Origin", url);

            // Call the next delegate/middleware in the pipeline
            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CorsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorsMiddleware>();
        }
    }
}

