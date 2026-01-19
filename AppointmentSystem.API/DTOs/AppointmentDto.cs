using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.API.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class CreateAppointmentDto
    {
        [Required]
        [MaxLength(200)]
        public string PatientName { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }

    public class UpdateAppointmentDto
    {
        [Required]
        [MaxLength(200)]
        public string PatientName { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }
}