using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class Login
    {
        [Required(ErrorMessage = "This field can't be empty")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string Password { get; set; }
    }
}
