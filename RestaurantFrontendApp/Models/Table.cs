using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class Table
    {
        public int TableID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public int TableSeats { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public bool IsAvailable { get; set; } 
    }
}
