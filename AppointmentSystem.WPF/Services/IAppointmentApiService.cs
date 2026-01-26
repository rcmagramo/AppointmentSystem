using AppointmentSystem.WPF.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AppointmentSystem.WPF.Services;

public interface IAppointmentApiService
{
    Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto?> GetAppointmentByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto> CreateAppointmentAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto> UpdateAppointmentAsync(
        int id,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAppointmentAsync(
        int id,
        CancellationToken cancellationToken = default);
}