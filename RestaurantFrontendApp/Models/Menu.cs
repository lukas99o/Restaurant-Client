using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class Menu
    {
        public int MenuID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string Name { get; set; }
    }
}
