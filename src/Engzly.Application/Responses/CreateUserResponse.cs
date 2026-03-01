namespace Engzly.Application.Responses
{
    public sealed class CreateUserResponse
    {
        public string UserId { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
