using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Application.Features.Auth.DTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100, ErrorMessage = "Full name must be less than 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Client URL is required")]
        //[MaxLength(255, ErrorMessage = "Client URL must be less than 255 characters")]
        //public string ClientUrl { get; set; } = string.Empty;
    }
}