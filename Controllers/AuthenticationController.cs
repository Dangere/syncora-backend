
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Models.DTOs.Auth;
using TaskManagementWebAPI.Services;

namespace TaskManagementWebAPI.Controllers;


// This controller doesn't have authorization 
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> LoginWithEmailAndPassword([FromBody] LoginRequestDTO loginRequest)
    {
        AuthenticationResponseDTO? loginResponseDTO = await _authService.LoginWithEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (loginResponseDTO == null)
            return BadRequest("Invalid credentials.");

        return Ok(loginResponseDTO);

    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterWithEmailAndPassword([FromBody] RegisterRequestDTO registerRequest)
    {
        AuthenticationResponseDTO? registerResponseDTO = await _authService.RegisterWithEmailAndPassword(registerRequest.Email, registerRequest.Password, registerRequest.UserName);

        if (registerResponseDTO == null)
            return BadRequest("Failed to register user.");

        return Ok(registerResponseDTO);

    }

}