using AppointmentSystem.Domain.Common;
using AppointmentSystem.Domain.Enums;
using AppointmentSystem.Domain.Exceptions;

namespace AppointmentSystem.Domain.Entities;

public class Appointment : BaseEntity
{
    // Private fields for encapsulation
    private string _patientName;
    private DateTime _appointmentDate;
    private string? _description;
    private AppointmentStatus _status;

    // Private constructor - forces use of factory method
    private Appointment() { }

    // Factory method with business rules
    public static Appointment Create(
        string patientName,
        DateTime appointmentDate,
        string? description = null)
    {
        var appointment = new Appointment();
        appointment.SetPatientName(patientName);
        appointment.SetAppointmentDate(appointmentDate);
        appointment.SetDescription(description);
        appointment._status = AppointmentStatus.Scheduled;
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        appointment.AddDomainEvent(new AppointmentCreatedEvent(appointment));

        return appointment;
    }

    // Read-only properties
    public string PatientName => _patientName;
    public DateTime AppointmentDate => _appointmentDate;
    public string? Description => _description;
    public AppointmentStatus Status => _status;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Business methods
    public void Complete()
    {
        if (_status == AppointmentStatus.Cancelled)
            throw new DomainException("Cannot complete a cancelled appointment");

        if (_status == AppointmentStatus.Completed)
            throw new DomainException("Appointment is already completed");

        _status = AppointmentStatus.Completed;
        UpdateModifiedTimestamp();
        AddDomainEvent(new AppointmentCompletedEvent(this));
    }

    public void Cancel(string reason)
    {
        if (_status == AppointmentStatus.Completed)
            throw new DomainException("Cannot cancel a completed appointment");

        _status = AppointmentStatus.Cancelled;
        UpdateModifiedTimestamp();
        AddDomainEvent(new AppointmentCancelledEvent(this, reason));
    }

    public void UpdatePatientName(string patientName)
    {
        SetPatientName(patientName);
        UpdateModifiedTimestamp();
    }

    public void UpdateAppointmentDate(DateTime appointmentDate)
    {
        if (_status == AppointmentStatus.Completed)
            throw new DomainException("Cannot modify date of completed appointment");

        SetAppointmentDate(appointmentDate);
        UpdateModifiedTimestamp();
    }

    // Private validation methods
    private void SetPatientName(string patientName)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new DomainException("Patient name cannot be empty");

        if (patientName.Length > 200)
            throw new DomainException("Patient name cannot exceed 200 characters");

        _patientName = patientName.Trim();
    }

    private void SetAppointmentDate(DateTime appointmentDate)
    {
        if (appointmentDate < DateTime.UtcNow.AddMinutes(-5))
            throw new DomainException("Cannot create appointment in the past");

        _appointmentDate = appointmentDate;
    }

    private void SetDescription(string? description)
    {
        if (description?.Length > 1000)
            throw new DomainException("Description cannot exceed 1000 characters");

        _description = description?.Trim();
    }

    private void UpdateModifiedTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

// Domain events
public record AppointmentCreatedEvent(Appointment Appointment) : IDomainEvent;
public record AppointmentCompletedEvent(Appointment Appointment) : IDomainEvent;
public record AppointmentCancelledEvent(Appointment Appointment, string Reason) : IDomainEvent;