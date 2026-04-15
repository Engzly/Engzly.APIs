namespace Engzly.Application.Responses.UsersResponse
{
    public sealed class CreateUserResponse
    {
        public string UserId { get; set; } = null!;
        public bool RequiresEmailVerification { get; set; } = true;
    }
}
