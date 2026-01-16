using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string Name { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }
    }
}
