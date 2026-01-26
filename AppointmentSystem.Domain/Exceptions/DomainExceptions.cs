namespace AppointmentSystem.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class AppointmentNotFoundException : DomainException
{
    public int AppointmentId { get; }

    public AppointmentNotFoundException(int appointmentId)
        : base($"Appointment with ID {appointmentId} was not found")
    {
        AppointmentId = appointmentId;
    }
}