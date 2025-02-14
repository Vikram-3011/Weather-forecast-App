using BlazorApp.Controllers;
using Microsoft.AspNetCore.Mvc;

[Route("api/notifications")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly DatabaseController _databaseController;

    public NotificationController(DatabaseController databaseController)
    {
        _databaseController = databaseController;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterToken([FromBody] UserFCMToken request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserEmail) || string.IsNullOrEmpty(request.FCMToken))
            return BadRequest("Invalid token data");

        await _databaseController.StoreFCMToken(request.UserEmail, request.FCMToken);
        return Ok(new { message = "Token stored successfully" });
    }
}
