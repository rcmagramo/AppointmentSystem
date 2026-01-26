using AppointmentSystem.Application.Common.Models;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppointmentSystem.Application.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler
    : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AppointmentDto>> Handle(
        CreateAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating appointment for patient {PatientName} on {AppointmentDate}",
            request.PatientName,
            request.AppointmentDate);

        // Use domain factory method
        var appointment = Appointment.Create(
            request.PatientName,
            request.AppointmentDate,
            request.Description);

        // Add through repository
        await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully created appointment {AppointmentId}",
            appointment.Id);

        // Map to DTO
        var dto = MapToDto(appointment);
        return Result.Success(dto);
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientName = appointment.PatientName,
            AppointmentDate = appointment.AppointmentDate,
            Description = appointment.Description,
            Status = appointment.Status.ToString(),
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}