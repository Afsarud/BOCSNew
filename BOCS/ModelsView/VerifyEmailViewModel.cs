using System.ComponentModel.DataAnnotations;

namespace BOCS.ModelsView
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}
