using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelTest.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public decimal GPD { get; set; }
        public decimal GRDA { get; set; }
        public decimal IPD { get; set; }
        public decimal IRDA { get; set; }
        public decimal Seizure { get; set; }
        public decimal Other { get; set; }
        public DateTime DateOfCreation { get; set; }

        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }
    }
}
