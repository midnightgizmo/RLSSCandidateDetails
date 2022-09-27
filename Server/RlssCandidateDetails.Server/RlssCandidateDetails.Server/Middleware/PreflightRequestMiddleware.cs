using Microsoft.Extensions.Options;
using RlssCandidateDetails.Server.Models;

namespace RlssCandidateDetails.Server.Middleware
{
    public class PreflightRequestMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// When the client sends a PreflightRequest (Sometimes known as OPTIONS)
        /// They will request to use a certin Header. We must respond with a 
        /// Access-Control-Allow-Headers that lists the headers we support from the
        /// ones they have requested we support
        /// </summary>
        private string[] AllowedPreflightRequestHeaders = new string[] { "authorization" };

        public PreflightRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IOptions<AppSettings> appSettings)
        {
            string? RequestMethod = httpContext.Request.Headers["Access-Control-Request-Method"].FirstOrDefault();
            if (RequestMethod != null && (RequestMethod == "OPTIONS" || RequestMethod == "GET" || RequestMethod == "POST"))
            {
                int i = 0;

                string[]? RequestHeaders = httpContext.Request.Headers["access-control-request-headers"].FirstOrDefault()?.Split(" ");

                if (RequestHeaders != null)
                {
                    List<string> HeaderWeAcceptFromThoseSent = new List<string>();

                    foreach (string aHeader in RequestHeaders)
                    {
                        // if we support the header the client is asking us to support
                        if (this.AllowedPreflightRequestHeaders.Contains(aHeader) == true)
                        {
                            HeaderWeAcceptFromThoseSent.Add(aHeader);
                        }

                    }
                    if (HeaderWeAcceptFromThoseSent.Count > 0)
                    {
                        httpContext.Response.Headers.Add("Access-Control-Allow-Headers", string.Join(',', HeaderWeAcceptFromThoseSent));
                    }

                    httpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");

                    httpContext.Response.StatusCode = 200;
                    return;
                }
            }

            // Call the next delegate/middleware in the pipeline
            await _next(httpContext);
        }
    }


    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class PreflightRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UsePreFlightRequestMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PreflightRequestMiddleware>();
        }
    }
}
