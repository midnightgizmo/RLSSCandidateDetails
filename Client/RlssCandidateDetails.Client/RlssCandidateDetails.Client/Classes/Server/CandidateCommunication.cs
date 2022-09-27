using RlssCandidateDetails.Client.Models;
using RlssCandidateDetails.Client.Models.Server;
using RlssCandidateDetails.Client.Models.Server.ResponseData;
using System.Text.Json;

namespace RlssCandidateDetails.Client.Classes.Server
{
	/// <summary>
	/// CRUD operations for comminicating with the database
	/// </summary>
	public class CandidateCommunication
	{
		private HttpClient _HttpClient;
		private ServerCommunication _ServerCommunication;
		public CandidateCommunication(HttpClient httpClient, AppSettings appSettings)
		{
			this._HttpClient = httpClient;
			this._ServerCommunication = new ServerCommunication(this._HttpClient, appSettings);
		}


		/// <summary>
		/// Gets all candidates that are stored in the database
		/// </summary>
		/// <returns></returns>
		public async Task<SeverResponseListofCandidates> GetAllCandidates()
		{
			ServerResponse responseMessage;
			SeverResponseListofCandidates responseData;

			// request all candidates details from the server
			responseMessage = await this._ServerCommunication.SendGetRequestToServer("Candidate/GetAllUsers");

			responseData = ServerCommunication.ParseServerResponse<SeverResponseListofCandidates>(responseMessage);

			return responseData;
			
		}

		public async Task<SeverResponseListofCandidates> GetFilterdCandidateList(Candidate candiateSearchDetails)
        {
			ServerResponse responseMessage;
			SeverResponseListofCandidates responseData;

			Dictionary<string, string> DataToSend = new Dictionary<string, string>();

			// We need to send the Id param but we dont want to use it so set it to zero
			DataToSend.Add("ID", "0");
			DataToSend.Add("FirstName", candiateSearchDetails.FirstName);
			DataToSend.Add("Surname", candiateSearchDetails.Surname);
			DataToSend.Add("SocietyNumber", candiateSearchDetails.SocietyNumber.ToString());
			DataToSend.Add("DateOfBirth", "");

			// request all candidates that match the search criteria
			responseMessage = await this._ServerCommunication.SendPostRequestToServer("Candidate/FindCandidates", DataToSend);

			responseData = ServerCommunication.ParseServerResponse<SeverResponseListofCandidates>(responseMessage);

			return responseData;
		}


		public async Task<ServerResponseSingleCandidate> GetSingleCandidate(int CandidateID)
        {
			ServerResponse responseMessage;
			ServerResponseSingleCandidate responseData;
			Dictionary<string, string> DataToSend = new Dictionary<string, string>();

			DataToSend.Add("Id", CandidateID.ToString());

			responseMessage = await this._ServerCommunication.SendPostRequestToServer("Candidate/GetCandidate",DataToSend);
			// get candidate data
			responseData = ServerCommunication.ParseServerResponse<ServerResponseSingleCandidate>(responseMessage);

			return responseData;
        }


		public async Task<ServerResponseSingleCandidate> UpdateCandidateDetails(Candidate CandidateDetails)
		{
			ServerResponse responseMessage;
			ServerResponseSingleCandidate responseData;
			Dictionary<string, string> DataToSend = new Dictionary<string, string>();

			DataToSend.Add("Id", CandidateDetails.Id.ToString());
			DataToSend.Add("FirstName", CandidateDetails.FirstName);
			DataToSend.Add("Surname", CandidateDetails.Surname);
			DataToSend.Add("SocietyNumber", CandidateDetails.SocietyNumber.ToString());
			DataToSend.Add("DateOfBirth", "");


			// update candidate details on server and request updated candidates details from the server
			responseMessage = await this._ServerCommunication.SendPostRequestToServer("Candidate/Update", DataToSend);

			responseData = ServerCommunication.ParseServerResponse<ServerResponseSingleCandidate>(responseMessage);

			return responseData;
		}

		/// <summary>
		/// Adds a new candidate to the database
		/// </summary>
		/// <param name="CandidateData"></param>
		/// <returns></returns>
		public async Task<ServerResponseSingleCandidate> InsertCandidate(Candidate CandidateData)
		{
			ServerResponse responseMessage;
			ServerResponseSingleCandidate responseData;
			Dictionary<string, string> DataToSend = new Dictionary<string, string>();

			DataToSend.Add("FirstName", CandidateData.FirstName);
			DataToSend.Add("Surname", CandidateData.Surname);
			DataToSend.Add("SocietyNumber", CandidateData.SocietyNumber.ToString());
			DataToSend.Add("DateOfBirth", "");


			// add new candidate to the server and request candidates details from the server
			responseMessage = await this._ServerCommunication.SendPostRequestToServer("Candidate/Insert", DataToSend);

			responseData = ServerCommunication.ParseServerResponse<ServerResponseSingleCandidate>(responseMessage);

			return responseData;
		}

		public async Task<ServerResponseData> DeleteCandidate(int CandidateId)
		{
			ServerResponse responseMessage;
			ServerResponseData responseData;
			Dictionary<string, string> DataToSend = new Dictionary<string, string>();

			DataToSend.Add("Id", CandidateId.ToString());


			// request all candidates details from the server
			responseMessage = await this._ServerCommunication.SendPostRequestToServer("Candidate/Delete", DataToSend);

			responseData = ServerCommunication.ParseServerResponse<ServerResponseData>(responseMessage);

			return responseData;
		}



	}
}
