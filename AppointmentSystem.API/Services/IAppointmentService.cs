using AppointmentSystem.API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppointmentSystem.API.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto> GetAppointmentByIdAsync(int id);
        Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto);
        Task<AppointmentDto> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto);
        Task<bool> DeleteAppointmentAsync(int id);
    }
}