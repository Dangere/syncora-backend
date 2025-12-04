using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SyncoraBackend.Pages.Auth;

public class PasswordResetModel(ILogger<PasswordResetModel> logger) : PageModel
{
    private readonly ILogger<PasswordResetModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = null!;


    public class InputModel
    {
        [Required(AllowEmptyStrings = false), DataType(DataType.Password), Display(Name = "New Password"), StringLength(32, MinimumLength = 6),]
        public string Password { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = false), DataType(DataType.Password), Display(Name = "Confirm Password"), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]

        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet()
    {
        if (string.IsNullOrEmpty(Token))
        {
            Response.Redirect("/index");
        }

    }


    public void OnPost()
    {

        if (!ModelState.IsValid)
        {
            return;
        }


        Console.WriteLine("Your password is " + Input.Password);

    }
}
