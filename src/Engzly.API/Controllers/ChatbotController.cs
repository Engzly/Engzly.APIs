using Engzly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbot;

    public ChatbotController(IChatbotService chatbot)
    {
        _chatbot = chatbot;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var sessionId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized(new { message = "User session could not be identified." });

        var result = await _chatbot.SendMessageAsync(request.Message, sessionId);
        return Ok(result);
    }

    [HttpPost("voice")]
    public async Task<IActionResult> SendVoice([FromForm] VoiceRequest request)
    {
        using var ms = new MemoryStream();
        await request.Audio.CopyToAsync(ms);
        var audioBytes = ms.ToArray();

        var sessionId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized(new { message = "User session could not be identified." });

        var result = await _chatbot.SendVoiceAsync(audioBytes, sessionId, request.SpeakReply);
        return Ok(result);
    }

    [HttpGet("topics")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTopics()
    {
        var result = await _chatbot.GetTopicsAsync();
        return Ok(result);
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetSession()
    {
        var sessionId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized(new { message = "User session could not be identified." });

        await _chatbot.ResetSessionAsync(sessionId);
        return Ok(new { status = "reset" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteSession()
    {
        var sessionId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized(new { message = "User session could not be identified." });

        await _chatbot.DeleteSessionAsync(sessionId);
        return Ok(new { status = "deleted" });
    }
}

public record SendMessageRequest(string Message);
public record VoiceRequest(IFormFile Audio, bool SpeakReply = true);