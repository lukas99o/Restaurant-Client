using System.ComponentModel.DataAnnotations;

namespace ResturangFrontEnd.Models
{
    public class BookTableViewModel
    {
        public DateOnly? Date { get; set; }
        
        public int? SelectedHour { get; set; }
        
        public List<Table> AvailableTables { get; set; } = new List<Table>();
        
        public List<int> SeatOptions { get; set; } = new List<int>();
        
        public DateTime? TimeStartUtc { get; set; }
        
        public DateTime? TimeEndUtc { get; set; }
    }
}
