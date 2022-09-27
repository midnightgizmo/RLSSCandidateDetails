using RlssCandidateDetails.Client.Models.Server;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using RlssCandidateDetails.Client.Models.Server.ResponseData;
using System.Text.Json;
using RlssCandidateDetails.Client.Models;

namespace RlssCandidateDetails.Client.Classes.Server
{
    public class ServerCommunication
    {
        /// <summary>
        /// Inishalize the class with the HttpClient
        /// </summary>
        /// <param name="httpClient">Should have its Base address to the API url location</param>
        /// <param name="appSettings"></param>
        public ServerCommunication(HttpClient httpClient, AppSettings appSettings)
        {
            this.httpClient = httpClient;
            // might hold the jwt that we will want to send back with every request to the server
            this.appSettings = appSettings;
        }


        /// <summary>
        /// Used to communicate with the server
        /// </summary>
        public HttpClient httpClient { get; }

        public AppSettings appSettings { get; }



        /// <summary>
        /// Sends a GET request to the server
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<ServerResponse> SendGetRequestToServer(string url)
        {
            // what we return from this method
            ServerResponse response = new ServerResponse();
            HttpResponseMessage httpResponseMessage = null;

            try
            {
                HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Get, url);

                // SetBrowserRequestCredentials is an extention method. make sure you have the following using statment
                // at the top of your code
                //using Microsoft.AspNetCore.Components.WebAssembly.Http;
                // SetBrowserRequestCredentials will make it so the browser will accept any cookies that have
                // been sent from the server (setCookie header). This is only needed because of CORS
#if DEBUG
                requestMsg.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
#endif
                // adds a jwt Bearer Authorization header to request if jwt is found in appSettings
                this.AddJwtToRequest();

                // send the get request
                httpResponseMessage = await this.httpClient.SendAsync(requestMsg);

            }
            catch (Exception e)
            {

                response.WereErrors = true;
                response.ResponseMessage = e.Message;
            }


            // make sure we got status code 200
            if (httpResponseMessage != null && httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;

                // this will get the response message
                var stream = httpResponseMessage.Content.ReadAsStreamAsync().Result;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    response.ResponseMessage = reader.ReadToEnd();
                }


            }
            // some status code other than 200
            else
            {
                response.StatusCode = httpResponseMessage.StatusCode;
                response.WereErrors = true;

                // this will get the response message
                var stream = httpResponseMessage.Content.ReadAsStreamAsync().Result;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    response.ResponseMessage = reader.ReadToEnd();
                }

            }


            return response;
        }



        /// <summary>
        /// Sends a POST requst to the server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data">The data to send as key value pairs</param>
        /// <returns></returns>
        public Task<ServerResponse> SendPostRequestToServer(string url, Dictionary<string, string> data)
        {
            return this.SendPostRequestToServer(url, data, "");
        }
        /// <summary>
        /// Used to send a POST request
        /// </summary>
        /// <param name="url">The location to send the request</param>
        /// <param name="data">the data to send as key value pairs</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<ServerResponse> SendPostRequestToServer(string url, Dictionary<string, string> data, string contentType)
        {
            // what we return from this method
            ServerResponse response = new ServerResponse();

            //FormUrlEncodedContent formUrlEncodedContent;
            HttpResponseMessage httpResponseMessage = null;

            // we are going to send the data back as a form (sets the content-type to application/x-www-form-urlencoded)
            using (FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(data))
            {

                try
                {
                    //Microsoft.AspNetCore.Components.WebAssembly.Http.WebAssemblyHttpRequestMessageExtensions.SetBrowserRequestCredentials()

                    HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMsg.Content = formUrlEncodedContent;
                    // SetBrowserRequestCredentials is an extention method. make sure you have the following using statment
                    // at the top of your code
                    //using Microsoft.AspNetCore.Components.WebAssembly.Http;
                    // SetBrowserRequestCredentials will make it so the browser will accept any cookies that have
                    // been sent from the server (setCookie header). This is only needed because of CORS
                    requestMsg.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                    //requestMsg.SetBrowserRequestMode(BrowserRequestMode.NoCors);

                    // adds a jwt Bearer Authorization header to request if jwt is found in appSettings
                    this.AddJwtToRequest();

                    httpResponseMessage = await this.httpClient.SendAsync(requestMsg);
                    // send a post request back to the server and get the response
                    //httpResponseMessage = await this.httpClient.PostAsync(url, formUrlEncodedContent);
                }
                catch (Exception e)
                {
                    response.WereErrors = true;
                    response.ResponseMessage = e.Message;
                }


                // make sure we got status code 200
                if (httpResponseMessage != null && httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.StatusCode = System.Net.HttpStatusCode.OK;


                    // this does not work, it returns null
                    //response.data = await httpResponseMessage.Content.ReadAsStringAsync();

                    // this will get the response message
                    var stream = httpResponseMessage.Content.ReadAsStreamAsync().Result;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                    {
                        response.ResponseMessage = reader.ReadToEnd();
                    }


                }
                // some status code other than 200
                else
                {

                    response.StatusCode = httpResponseMessage.StatusCode;
                    response.WereErrors = true;

                    // this will get the response message
                    var stream = httpResponseMessage.Content.ReadAsStreamAsync().Result;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                    {
                        response.ResponseMessage = reader.ReadToEnd();
                    }


                }




                return response;
            }


        }

        public static T ParseServerResponse<T>(ServerResponse ResponseMessage) where T : ServerResponseData, new()
		{
            T ResponseData = new T();
            if (ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ResponseData = JsonSerializer.Deserialize<T>(ResponseMessage.ResponseMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ResponseData.StatusCode = ResponseMessage.StatusCode;
                
            }
            // some error must of occured
            else
            {
                // see if the server has given us any information about the error
                if (ResponseMessage.ResponseMessage != null && ResponseMessage.ResponseMessage.Length > 0)
                {
                    ResponseData = JsonSerializer.Deserialize<T>(ResponseMessage.ResponseMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                // no information from the server, don't know whats happend
                else
                {
                    ResponseData = new T();
                    ResponseData.Errors.Add("Unknown error has occured");
                }

                ResponseData.StatusCode = ResponseMessage.StatusCode;
                ResponseData.HasErrors = true;

                switch(ResponseData.StatusCode)
                {
                    case System.Net.HttpStatusCode.Forbidden:

                        ResponseData.Errors.Add("You do dot have access to that request");
                        Console.WriteLine("403 Forbidden: You do not have the correct role for that request");
                        break;
                }
            }

            return ResponseData;
        }


        /// <summary>
        /// If JWT found in appSettings. Authorization: Bearer Header will be added
        /// to request sent to the server with the jwt set as the value
        /// </summary>
        private void AddJwtToRequest()
        {
            // if we have a json web token, create an Authorization header of type "Bearer" and set its value as the json web token
            if (this.appSettings.JsonWebToken != null && this.appSettings.JsonWebToken.Length > 0)
            {
                this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.appSettings.JsonWebToken);
                //this.httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.appSettings.JsonWebToken);
            }

            return;
        }


    }
}
