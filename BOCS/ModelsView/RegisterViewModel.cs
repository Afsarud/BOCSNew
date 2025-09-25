using System.ComponentModel.DataAnnotations;

namespace BOCS.ModelsView
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required!")]
        [StringLength(12, MinimumLength =6, ErrorMessage = "The {0} must be at {2} and at max {1} charecter long!")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match!")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Password is required!")]
        [DataType(DataType.Password)]
        [Display(Name="Confirm Password")]
        public string ConfirmPassword { get; set; }
        [Required]
        [Display(Name = "Select Role")]
        public string Role { get; set; }
    }

}
