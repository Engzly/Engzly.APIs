namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class SubmitProposalResponse
    {
        public string Message { get; set; }
        public string ApplicationId { get; set; }

        public SubmitProposalResponse(string message, string applicationId)
        {
            Message = message;
            ApplicationId = applicationId;
        }
    }
}
