using AppointmentSystem.API.DTOs;
using AppointmentSystem.API.Models;
using AppointmentSystem.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentSystem.API.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;

        public AppointmentService(IAppointmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _repository.GetAllAsync();
            return appointments.Select(MapToDto);
        }

        public async Task<AppointmentDto> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            return appointment != null ? MapToDto(appointment) : null;
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto)
        {
            var appointment = new Appointment
            {
                PatientName = dto.PatientName,
                AppointmentDate = dto.AppointmentDate,
                Description = dto.Description,
                Status = string.IsNullOrEmpty(dto.Status) ? "Scheduled" : dto.Status
            };

            var created = await _repository.CreateAsync(appointment);
            return MapToDto(created);
        }

        public async Task<AppointmentDto> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.PatientName = dto.PatientName;
            existing.AppointmentDate = dto.AppointmentDate;
            existing.Description = dto.Description;
            existing.Status = dto.Status;

            var updated = await _repository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                PatientName = appointment.PatientName,
                AppointmentDate = appointment.AppointmentDate,
                Description = appointment.Description,
                Status = appointment.Status
            };
        }
    }
}