using RlssCandidateDetails.Client.Models;
using RlssCandidateDetails.Client.Models.Server;
using RlssCandidateDetails.Client.Models.Server.ResponseData;

namespace RlssCandidateDetails.Client.Classes.Server
{
	public class AuthenticationCommunication
	{
		private HttpClient _HttpClient;
		private ServerCommunication _ServerCommunication;
		public AuthenticationCommunication(HttpClient httpClient, AppSettings appSettings)
		{
			this._HttpClient = httpClient;
			this._ServerCommunication = new ServerCommunication(this._HttpClient,appSettings);
		}


		/// <summary>
		/// Send login detila to the server and ask if we can login
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <returns></returns>
		public async Task<SereverResponsePlainText> Login(string UserName, string Password)
		{
			ServerResponse responseMessage;
			SereverResponsePlainText responseData;

			Dictionary<string, string> DataToSend = new Dictionary<string, string>();

			// We need to send the Id param but we dont want to use it so set it to zero
			DataToSend.Add("username", UserName);
			DataToSend.Add("password", Password);


			// request all candidates that match the search criteria
			responseMessage = await this._ServerCommunication.SendPostRequestToServer("Authentication/Login", DataToSend);
			
			responseData = ServerCommunication.ParseServerResponse<SereverResponsePlainText>(responseMessage);

			return responseData;
		}

		public async Task<SereverResponsePlainText> RefreshToken()
        {
			ServerResponse responseMessage;
			SereverResponsePlainText responseData;

			// sends the refresh cookie to the server
			responseMessage = await this._ServerCommunication.SendGetRequestToServer("Authentication/RefreshToken");

			responseData = ServerCommunication.ParseServerResponse<SereverResponsePlainText>(responseMessage);

			return responseData;
		}
	}
}
