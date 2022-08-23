using RlssCandidateDetails.Client.Models.Server;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using RlssCandidateDetails.Client.Models.Server.ResponseData;
using System.Text.Json;

namespace RlssCandidateDetails.Client.Classes.Server
{
    public class ServerCommunication
    {
        /// <summary>
        /// Inishalize the class with the HttpClient
        /// </summary>
        /// <param name="httpClient">Should have its Base address to the API url location</param>
        public ServerCommunication(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }


        /// <summary>
        /// Used to communicate with the server
        /// </summary>
        public HttpClient httpClient { get; }



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

        public static T ParseServerResponse<T>(ServerResponse ResponseMessage) where T : ServerResponseDataBase, new()
		{
            T ResponseData = new T();
            if (ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ResponseData = JsonSerializer.Deserialize<T>(ResponseMessage.ResponseMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ResponseData.StatusCode = ResponseData.StatusCode;
                
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
            }

            return ResponseData;
        }



    }
}
