using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SyncoraBackend.Data;
using SyncoraBackend.Migrations;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Services;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Pages.Auth;

public class PasswordResetModel(ILogger<PasswordResetModel> logger, AuthService authService) : PageModel
{
    private readonly ILogger<PasswordResetModel> _logger = logger;
    private readonly AuthService _authService = authService;

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = null!;

    [TempData]
    public string? Message { get; set; }


    public class InputModel
    {
        [Required(AllowEmptyStrings = false), DataType(DataType.Password), Display(Name = "New Password"), StringLength(32, MinimumLength = 6),]
        public string Password { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = false), DataType(DataType.Password), Display(Name = "Confirm Password"), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]

        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGet()
    {
        Message = null;
        if (string.IsNullOrEmpty(Token))
        {
            Console.WriteLine("Token is empty");
            Message = "Invalid password reset token. Expired or already consumed.";
        }

        Result<string> result = await _authService.ValidatePasswordResetToken(Token);

        if (!result.IsSuccess)
        {
            Message = result.ErrorMessage!;
        }

        return Page();

    }


    public async Task<IActionResult> OnPost()
    {
        // Check if model is valid
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // This both validates the token and updates the password if token is valid and consumes it
        Result<string> result = await _authService.UpdateUserPassword(Token, Input.Password);
        if (!result.IsSuccess)
            Message = result.ErrorMessage;
        else
            Message = "Password changed!";


        return Page();
    }
}
