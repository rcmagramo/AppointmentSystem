using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Domain.Repositories;
using AppointmentSystem.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppointmentDbContext _context;

    public AppointmentRepository(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Appointment>> GetBySpecificationAsync(
        ISpecification<Appointment> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Appointment> Items, int TotalCount)> GetPagedAsync(
        ISpecification<Appointment> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await CountAsync(specification, cancellationToken);

        var items = await ApplySpecification(specification)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountAsync(
        ISpecification<Appointment> specification,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Appointment> AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
        return appointment;
    }

    public void Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
    }

    public void Delete(Appointment appointment)
    {
        _context.Appointments.Remove(appointment);
    }

    private IQueryable<Appointment> ApplySpecification(ISpecification<Appointment> specification)
    {
        var query = _context.Appointments.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}