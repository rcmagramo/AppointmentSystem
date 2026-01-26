using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Domain.Specifications;

namespace AppointmentSystem.Domain.Repositories;

public interface IAppointmentRepository
{
    // Query methods
    Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Appointment>> GetBySpecificationAsync(
        ISpecification<Appointment> specification,
        CancellationToken cancellationToken = default);

    Task<(List<Appointment> Items, int TotalCount)> GetPagedAsync(
        ISpecification<Appointment> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        ISpecification<Appointment> specification,
        CancellationToken cancellationToken = default);

    // Command methods
    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    void Update(Appointment appointment);
    void Delete(Appointment appointment);
}

public interface IUnitOfWork : IDisposable
{
    IAppointmentRepository Appointments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}