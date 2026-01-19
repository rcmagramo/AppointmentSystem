using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.API.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string PatientName { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}