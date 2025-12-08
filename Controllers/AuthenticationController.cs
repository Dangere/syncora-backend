
using SyncoraBackend.Utilities;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Models.DTOs.Auth;
using SyncoraBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;

namespace SyncoraBackend.Controllers;


// This controller doesn't have authorization, can be accessed by anyone

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth-policy")]
public class AuthenticationController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    private const string _verifyEmailEndpointName = "VerifyEmail";


    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> LoginWithEmailAndPassword([FromBody] LoginRequestDTO loginRequest)
    {
        Result<AuthenticationResponseDTO> loginResult = await _authService.LoginWithEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (!loginResult.IsSuccess)
            return StatusCode(loginResult.ErrorStatusCode, loginResult.ErrorMessage);


        return Ok(loginResult.Data);

    }

    [AllowAnonymous, HttpPost("register")]
    public async Task<IActionResult> RegisterWithEmailAndPassword([FromBody] RegisterRequestDTO registerRequest)
    {
        string? verifyUrl = Url.Link(
    routeName: _verifyEmailEndpointName,
    null
);
        if (verifyUrl == null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Could not generate verification URL" });
        Result<AuthenticationResponseDTO> registerResult = await _authService.RegisterWithEmailAndPassword(registerRequest, verifyUrl);

        if (!registerResult.IsSuccess)
            return StatusCode(registerResult.ErrorStatusCode, registerResult.ErrorMessage);


        return Ok(registerResult.Data);

    }

    [AllowAnonymous, HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokensDTO tokens)
    {

        Result<TokensDTO> refreshTokenResult = await _authService.RefreshToken(expiredAccessToken: tokens.AccessToken, refreshToken: tokens.RefreshToken);

        if (!refreshTokenResult.IsSuccess)
            return StatusCode(refreshTokenResult.ErrorStatusCode, refreshTokenResult.ErrorMessage);

        return Ok(refreshTokenResult.Data);
        // This will return a new access token and a new refresh token
        // We return a refresh token to make sure its rotated in case of a security issue it will log out the user and detect token theft

    }


    [AllowAnonymous, HttpPost("login/google/{idToken}")]
    public async Task<IActionResult> LoginWithGoogle(string idToken)
    {
        Result<AuthenticationResponseDTO> loginResult = await _authService.LoginWithGoogle(idToken);

        if (!loginResult.IsSuccess)
            return StatusCode(loginResult.ErrorStatusCode, loginResult.ErrorMessage);

        return Ok(loginResult.Data);
    }
    [AllowAnonymous, HttpPost("register/google")]
    public async Task<IActionResult> RegisterWithGoogle(RegisterWithGoogleRequestDTO registerWithGoogleRequest)
    {
        string? verifyUrl = Url.Link(
            routeName: _verifyEmailEndpointName,
            null
        );
        if (verifyUrl == null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Could not generate verification URL" });

        Result<AuthenticationResponseDTO> registerResult = await _authService.RegisterWithGoogle(registerWithGoogleRequest, verifyUrl);

        if (!registerResult.IsSuccess)
            return StatusCode(registerResult.ErrorStatusCode, registerResult.ErrorMessage);

        return Ok(registerResult.Data);
    }

    [AllowAnonymous, HttpGet("verify", Name = _verifyEmailEndpointName)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        Result<string> verifyResult = await _authService.ConfirmVerificationEmail(token);

        if (!verifyResult.IsSuccess)
            return StatusCode(verifyResult.ErrorStatusCode, verifyResult.ErrorMessage);


        return Ok(verifyResult.Data);

    }

    [AuthorizeRoles(UserRole.User, UserRole.Admin), HttpPost("verify/send"), EnableRateLimiting("email-policy")]
    public async Task<IActionResult> SendEmailVerification()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        string? verifyUrl = Url.Link(
            routeName: _verifyEmailEndpointName,
            null
        );
        if (verifyUrl == null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Could not generate verification URL" });

        Result<string> emailResult = await _authService.SendVerificationEmail(userId, verifyUrl);

        if (!emailResult.IsSuccess)
            return StatusCode(emailResult.ErrorStatusCode, emailResult.ErrorMessage);


        return Ok(emailResult.Data);
    }


    [AuthorizeRoles(UserRole.User, UserRole.Admin), HttpPost("password-reset/send"), EnableRateLimiting("email-policy")]
    public async Task<IActionResult> SendEmailPasswordReset()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        string? passwordRestPageUrl = Url.Page(pageName: "/Auth/PasswordReset", values
        : null, pageHandler: null, protocol: Request.Scheme, host: Request.Host.ToString()
        );


        if (passwordRestPageUrl == null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Could not generate password reset page URL" });

        Result<string> emailResult = await _authService.SendPasswordResetEmail(userId, passwordRestPageUrl);

        if (!emailResult.IsSuccess)
            return StatusCode(emailResult.ErrorStatusCode, emailResult.ErrorMessage);


        return Ok(emailResult.Data);
    }
}