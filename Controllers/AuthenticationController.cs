
using SyncoraBackend.Utilities;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Models.DTOs.Auth;
using SyncoraBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace SyncoraBackend.Controllers;


// This controller doesn't have authorization, can be accessed by anyone
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth-policy")]
public class AuthenticationController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> LoginWithEmailAndPassword([FromBody] LoginRequestDTO loginRequest)
    {
        Result<AuthenticationResponseDTO> loginResult = await _authService.LoginWithEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (!loginResult.IsSuccess)
            return StatusCode(loginResult.ErrorStatusCode, loginResult.ErrorMessage);


        return Ok(loginResult.Data);

    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterWithEmailAndPassword([FromBody] RegisterRequestDTO registerRequest)
    {
        Result<AuthenticationResponseDTO> registerResult = await _authService.RegisterWithEmailAndPassword(registerRequest.Email, registerRequest.Password, registerRequest.Username);

        if (!registerResult.IsSuccess)
            return StatusCode(registerResult.ErrorStatusCode, registerResult.ErrorMessage);


        return Ok(registerResult.Data);

    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokensDTO tokens)
    {

        Result<TokensDTO> refreshTokenResult = await _authService.RefreshToken(expiredAccessToken: tokens.AccessToken, refreshToken: tokens.RefreshToken);

        if (!refreshTokenResult.IsSuccess)
            return StatusCode(refreshTokenResult.ErrorStatusCode, refreshTokenResult.ErrorMessage);

        return Ok(refreshTokenResult.Data);
        // This will return a new access token and a new refresh token
        // We return a refresh token to make sure its rotated in case of a security issue it will log out the user and detect token theft

    }


    [HttpPost("login/google/{idToken}")]
    public async Task<IActionResult> LoginWithGoogle(string idToken)
    {
        Result<AuthenticationResponseDTO> loginResult = await _authService.LoginWithGoogle(idToken);

        if (!loginResult.IsSuccess)
            return StatusCode(loginResult.ErrorStatusCode, loginResult.ErrorMessage);

        return Ok(loginResult.Data);
    }
    [HttpPost("register/google")]
    public async Task<IActionResult> RegisterWithGoogle(RegisterWithGoogleRequestDTO registerWithGoogleRequest)
    {
        Result<AuthenticationResponseDTO> registerResult = await _authService.RegisterWithGoogle(registerWithGoogleRequest.IdToken, registerWithGoogleRequest.Username, registerWithGoogleRequest.Password);

        if (!registerResult.IsSuccess)
            return StatusCode(registerResult.ErrorStatusCode, registerResult.ErrorMessage);

        return Ok(registerResult.Data);
    }
}