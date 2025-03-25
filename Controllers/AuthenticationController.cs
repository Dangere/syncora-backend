
using TaskManagementWebAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Models.DTOs.Auth;
using TaskManagementWebAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagementWebAPI.Controllers;


// This controller doesn't have authorization, can be accessed by anyone
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> LoginWithEmailAndPassword([FromBody] LoginRequestDTO loginRequest)
    {
        Result<AuthenticationResponseDTO> loginResult = await _authService.LoginWithEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (!loginResult.IsSuccess)
            return BadRequest(loginResult.ErrorMessage);

        return Ok(loginResult.Data);

    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterWithEmailAndPassword([FromBody] RegisterRequestDTO registerRequest)
    {
        Result<AuthenticationResponseDTO> registerResult = await _authService.RegisterWithEmailAndPassword(registerRequest.Email, registerRequest.Password, registerRequest.UserName);

        if (!registerResult.IsSuccess)
            return BadRequest(registerResult.ErrorMessage);

        return Ok(registerResult.Data);

    }

}