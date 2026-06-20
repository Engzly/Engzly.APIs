namespace Engzly.Application.Responses.OTPResponse
{
    public sealed record VerifyOtpResponse
    (
        string UserId,
        string UserName,
        string Role,
        string AccessToken,
        string RefreshToken
        );

}
