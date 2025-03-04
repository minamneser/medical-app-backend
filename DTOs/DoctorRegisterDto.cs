using System.ComponentModel.DataAnnotations;

namespace ModelTest.DTOs
{
    public class DoctorRegisterDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
