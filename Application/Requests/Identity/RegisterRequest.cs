using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Application.Requests.Identity
{
    public class RegisterRequest
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string UserName { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        
        [Required, RegularExpression(@"^\d{10}$")]

        public string PhoneNumber { get; set; }

        public bool ActivateUser { get; set; } = true;

        public bool AutoConfirmEmail { get; set; } = false;

    }
}