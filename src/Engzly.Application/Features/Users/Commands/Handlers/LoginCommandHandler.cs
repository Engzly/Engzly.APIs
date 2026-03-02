using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Services;
using Engzly.Application.Responses.UsersResponse;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers;

public class LoginCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService) : ResponseHandler,
    IRequestHandler<LoginCommand, Response<LoginResponse>>
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    
    private const string InvalidEmailOrPassword = "Invalid Email or Password";

    public async Task<Response<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Check Email If Exists
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest<LoginResponse>(InvalidEmailOrPassword);

        // Check Status
        if (user.Status != UserStatus.Active)
            return BadRequest<LoginResponse>("Account is not Active");

        // Check password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (result.IsLockedOut)
            return BadRequest<LoginResponse>("Account is Locked. Try Again Later.");

        if (!result.Succeeded)
            return BadRequest<LoginResponse>(InvalidEmailOrPassword);

        // Generate Tokens
        var jwtToken = await _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);


        var response = new LoginResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            AccessToken = jwtToken,
            RefreshToken = refreshToken
        };


        return Success(response  , "Login Is Successfully");
    }
}