using AppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AppointmentSystem.Infrastructure.Persistence;

public class AppointmentDbContext : DbContext
{
    private readonly ILogger<AppointmentDbContext>? _logger;

    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
        : base(options) { }

    public AppointmentDbContext(
        DbContextOptions<AppointmentDbContext> options,
        ILogger<AppointmentDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}