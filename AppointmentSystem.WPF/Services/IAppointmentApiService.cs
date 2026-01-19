using AppointmentSystem.WPF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppointmentSystem.WPF.Services
{
    public interface IAppointmentApiService
    {
        Task<List<AppointmentModel>> GetAllAppointmentsAsync();
        Task<AppointmentModel> GetAppointmentByIdAsync(int id);
        Task<AppointmentModel> CreateAppointmentAsync(AppointmentModel appointment);
        Task<AppointmentModel> UpdateAppointmentAsync(int id, AppointmentModel appointment);
        Task<bool> DeleteAppointmentAsync(int id);
    }
}