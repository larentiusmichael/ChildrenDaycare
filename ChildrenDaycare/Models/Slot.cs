using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChildrenDaycare.Models
{
    public class Slot
    {
        [Key]   //primary key
        public int SlotID { get; set; }

        public DateTime SlotDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [ForeignKey("Id")]
        public string TakecareGiverID { get; set; }

        public bool isBooked { get; set; }

        public string ChildFullname { get; set; }

        public int ChildAge { get; set; }

        public DateTime ChildDOB { get; set; }

        public decimal SlotPrice { get; set; }

    }
}
