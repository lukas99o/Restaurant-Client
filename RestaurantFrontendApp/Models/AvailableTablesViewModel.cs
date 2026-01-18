using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class AvailableTablesViewModel
    {
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public int SelectedSeats { get; set; } = 2;

        public int? SelectedHour { get; set; }

        public DateTime? SelectedTimeUtc { get; set; }

        public DateTime? SelectedTimeEndUtc { get; set; }

        public List<int> AvailableHours { get; set; } = new();

        public List<TableGetDTO> AvailableTables { get; set; } = new();

        public bool ShowBookingForm { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Phone { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public int? SelectedTableID { get; set; }
    }
}
