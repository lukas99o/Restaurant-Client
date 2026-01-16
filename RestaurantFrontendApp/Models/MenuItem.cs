using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public int MenuID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string Name { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public int Price { get; set; }

        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string Description { get; set; }

        public string? ImgUrl { get; set; }
    }
}
