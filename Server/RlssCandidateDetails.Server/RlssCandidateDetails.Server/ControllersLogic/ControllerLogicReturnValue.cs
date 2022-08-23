namespace RlssCandidateDetails.Server.ControllersLogic
{
    public class ControllerLogicReturnValue
    {
        public object ReturnValue { get; set; } = null;
        public bool HasErrors { get; set; } = false;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
