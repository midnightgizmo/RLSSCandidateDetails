namespace RlssCandidateDetails.Client.Models.Server.ResponseData
{
	public class ServerResponseData
	{
		public System.Net.HttpStatusCode StatusCode { get; set; }
		public object ReturnValue { get; set; } = null;
		public bool HasErrors { get; set; } = false;
		public List<string> Errors { get; set; } = new List<string>();
	}
}
