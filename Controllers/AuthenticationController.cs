
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
        LoginResponseDTO? loginResponseDTO = await _authService.LoginWithEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (loginResponseDTO == null)
            return BadRequest("Invalid email or password.");

        return Ok(loginResponseDTO);

    }


}