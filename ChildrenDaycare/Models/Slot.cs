using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChildrenDaycare.Models
{
    public class Slot
    {
        [Key]   //primary key
        public int SlotID { get; set; }

        [Required(ErrorMessage = "Please select slot date!")]
        public DateTime SlotDate { get; set; }

        [Required(ErrorMessage = "Please select time!")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Please select time!")]
        public TimeSpan EndTime { get; set; }

        [ForeignKey("Id")]
        [Required(ErrorMessage = "Please select takecare giver!")]
        public string TakecareGiverID { get; set; }

        public bool isBooked { get; set; }

        public string? ChildFullname { get; set; }

        public int? ChildAge { get; set; }

        public DateTime? ChildDOB { get; set; }

        [Required(ErrorMessage = "Please enter price!")]
        public decimal SlotPrice { get; set; }

        [ForeignKey("Id")]
        public string? BookerID { get; set; }

    }
}
