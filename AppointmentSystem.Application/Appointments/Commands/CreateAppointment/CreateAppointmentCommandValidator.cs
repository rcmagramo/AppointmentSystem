using FluentValidation;

namespace AppointmentSystem.Application.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required")
            .MaximumLength(200).WithMessage("Patient name cannot exceed 200 characters")
            .Must(BeValidName).WithMessage("Patient name contains invalid characters");

        RuleFor(x => x.AppointmentDate)
            .NotEmpty().WithMessage("Appointment date is required")
            .Must(BeInFuture).WithMessage("Appointment date must be in the future")
            .Must(BeWithinBusinessHours).WithMessage("Appointments must be during business hours (8 AM - 5 PM)")
            .Must(NotBeWeekend).WithMessage("Appointments cannot be scheduled on weekends");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private bool BeValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) &&
               name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-' || c == '\'');
    }

    private bool BeInFuture(DateTime date)
    {
        return date > DateTime.UtcNow.AddMinutes(5);
    }

    private bool BeWithinBusinessHours(DateTime date)
    {
        var hour = date.Hour;
        return hour >= 8 && hour < 17;
    }

    private bool NotBeWeekend(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }
}