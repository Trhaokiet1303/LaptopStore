using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Application.Requests.Identity
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}