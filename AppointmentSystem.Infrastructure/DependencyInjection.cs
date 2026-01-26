using AppointmentSystem.Domain.Repositories;
using AppointmentSystem.Infrastructure.Persistence;
using AppointmentSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext with SQLite
        services.AddDbContext<AppointmentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(30);
            });

            //       options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Register repositories and Unit of Work
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}