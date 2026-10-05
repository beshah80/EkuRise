using EkubApi.DTOs;
using EkubApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // --- Registration ---

    /// <summary>
    /// Step 1: Submit phone + profile data. Creates an unverified account and sends OTP.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OtpSentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OtpSentDto>> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Step 2: Verify the OTP code to activate the account. Returns JWT.
    /// </summary>
    [HttpPost("register/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> VerifyRegistration([FromBody] VerifyRegistrationDto dto)
    {
        var result = await _authService.VerifyRegistrationCodeAsync(dto);
        return Ok(result);
    }

    // --- Login ---

    /// <summary>
    /// Send a login OTP to the phone number.
    /// </summary>
    [HttpPost("login/otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OtpSentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OtpSentDto>> SendOtp([FromBody] SendOtpDto dto)
    {
        var result = await _authService.SendLoginOtpAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Verify the login OTP. Returns JWT.
    /// </summary>
    [HttpPost("login/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var result = await _authService.VerifyOtpAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Login with phone number + PIN (if PIN is enabled).
    /// </summary>
    [HttpPost("login/pin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> PinLogin([FromBody] PinLoginDto dto)
    {
        var result = await _authService.PinLoginAsync(dto);
        return Ok(result);
    }

    // --- Account ---

    /// <summary>
    /// Get the current user's full profile.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _authService.GetProfileAsync(userId);
        return Ok(profile);
    }

    /// <summary>
    /// Update the current user's profile fields.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var profile = await _authService.UpdateProfileAsync(userId, dto);
        return Ok(profile);
    }

    /// <summary>
    /// Set or change the login PIN.
    /// </summary>
    [HttpPost("me/pin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPin([FromBody] SetPinDto dto)
    {
        var userId = GetUserId();
        await _authService.SetPinAsync(userId, dto);
        return Ok(new { message = "PIN updated successfully." });
    }

    /// <summary>
    /// Step 1 of account deletion: request a verification code.
    /// </summary>
    [HttpPost("delete-account/request")]
    [ProducesResponseType(typeof(OtpSentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OtpSentDto>> RequestDeleteAccount([FromBody] DeleteAccountRequestDto dto)
    {
        var result = await _authService.RequestDeleteAccountAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Step 2 of account deletion: confirm with the verification code.
    /// </summary>
    [HttpPost("delete-account/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmDeleteAccount([FromBody] ConfirmDeleteAccountDto dto)
    {
        var userId = GetUserId();
        await _authService.ConfirmDeleteAccountAsync(userId, dto);
        return Ok(new { message = "Account deleted successfully." });
    }
}
