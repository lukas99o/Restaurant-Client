using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public int TableID { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public int AmountOfPeople { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public DateTime Time { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public DateTime TimeEnd { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "This field can't be empty")]
        public string? Email { get; set; }

        public int MaxSeats { get; set; }
    }
}
