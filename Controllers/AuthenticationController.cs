
using SyncoraBackend.Utilities;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Models.DTOs.Auth;
using SyncoraBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using System.ComponentModel.DataAnnotations;

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
        {

            return this.ErrorResponse(loginResult);
        }



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
            return this.ErrorResponse(
                Result<string>.Error("Could not generate verification URL", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError)
            );

        Result<AuthenticationResponseDTO> registerResult = await _authService.RegisterWithEmailAndPassword(registerRequest, verifyUrl);

        if (!registerResult.IsSuccess)
            return this.ErrorResponse(registerResult);



        return Ok(registerResult.Data);

    }

    [AllowAnonymous, HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokensDTO tokens)
    {

        Result<TokensDTO> refreshTokenResult = await _authService.RefreshToken(expiredAccessToken: tokens.AccessToken, refreshToken: tokens.RefreshToken);

        if (!refreshTokenResult.IsSuccess)
            return this.ErrorResponse(refreshTokenResult);


        return Ok(refreshTokenResult.Data);
        // This will return a new access token and a new refresh token
        // We return a refresh token to make sure its rotated in case of a security issue it will log out the user and detect token theft

    }


    [AllowAnonymous, HttpPost("login/google/{idToken}")]
    public async Task<IActionResult> LoginWithGoogle(string idToken)
    {
        Result<AuthenticationResponseDTO> loginResult = await _authService.LoginWithGoogle(idToken);

        if (!loginResult.IsSuccess)
            return this.ErrorResponse(loginResult);


        return Ok(loginResult.Data);
    }
    [AllowAnonymous, HttpPost("register/google")]
    public async Task<IActionResult> RegisterWithGoogle(RegisterWithGoogleRequestDTO registerWithGoogleRequest)
    {
        Result<AuthenticationResponseDTO> registerResult = await _authService.RegisterWithGoogle(registerWithGoogleRequest);

        if (!registerResult.IsSuccess)
            return this.ErrorResponse(registerResult);


        return Ok(registerResult.Data);
    }

    [AllowAnonymous, HttpGet("verify", Name = _verifyEmailEndpointName)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        Result<string> verifyResult = await _authService.ConfirmVerificationEmail(token);

        if (!verifyResult.IsSuccess)
            return this.ErrorResponse(verifyResult);



        return Ok(verifyResult.Data);

    }

    [AuthorizeRoles(UserRoles.User, UserRoles.Admin), HttpPost("verify/send"), EnableRateLimiting("email-policy")]
    public async Task<IActionResult> SendEmailVerification()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        string? verifyUrl = Url.Link(
            routeName: _verifyEmailEndpointName,
            null
        );
        if (verifyUrl == null)
            return this.ErrorResponse(
                Result<string>.Error("Could not generate verification URL", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError)
            );

        Result<string> emailResult = await _authService.SendVerificationEmail(userId, verifyUrl);

        if (!emailResult.IsSuccess)
            return this.ErrorResponse(emailResult);



        return Ok(emailResult.Data);
    }

    [AuthorizeRoles(UserRoles.User, UserRoles.Admin), HttpPost("verify/status")]
    public async Task<IActionResult> CheckVerificationStatus()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<bool> checkResult = await _authService.CheckUserVerification(userId);

        if (!checkResult.IsSuccess)
            return this.ErrorResponse(checkResult);



        return Ok(checkResult.Data);
    }



    [HttpPost("password-reset/send/{email}"), EnableRateLimiting("email-policy")]
    public async Task<IActionResult> SendEmailPasswordReset([EmailAddress] string email)
    {

        string? passwordRestPageUrl = Url.Page(pageName: "/Auth/PasswordReset", values
        : null, pageHandler: null, protocol: Request.Scheme, host: Request.Host.ToString()
        );


        if (passwordRestPageUrl == null)
            return this.ErrorResponse(
                 Result<string>.Error("Could not generate password reset page URL", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError)
            );

        Result<string> emailResult = await _authService.SendPasswordResetEmail(email, passwordRestPageUrl);

        if (!emailResult.IsSuccess)
            return this.ErrorResponse(emailResult);



        return Ok(emailResult.Data);
    }
}