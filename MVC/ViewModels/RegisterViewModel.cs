using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Name is required.")]
        public string UserName {get; set;}

        [Required(ErrorMessage ="Email is required.")]
        [EmailAddress]
        public string Email {get; set;}

        [Required(ErrorMessage ="Password is required.")]
        [StringLength(40, MinimumLength =6, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match")]
        public string Password {get; set;}
        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]

        public string ConfirmPassword {get; set;}

    }
}