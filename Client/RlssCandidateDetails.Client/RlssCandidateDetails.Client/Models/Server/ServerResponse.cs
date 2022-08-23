namespace RlssCandidateDetails.Client.Models.Server
{
    public class ServerResponse
    {
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public string ResponseMessage { get; set; }

        public bool WereErrors { get; set; }
    }
}
