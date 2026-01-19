using System;

namespace AppointmentSystem.WPF.Models
{
    public class AppointmentModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}