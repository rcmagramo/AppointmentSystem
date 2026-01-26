using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AppointmentDate)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Store enum as string for readability
        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (AppointmentStatus)Enum.Parse(typeof(AppointmentStatus), v));

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.PatientName);
        builder.HasIndex(a => a.AppointmentDate);
        builder.HasIndex(a => a.Status);

        // Ignore domain events (not persisted)
        builder.Ignore(a => a.DomainEvents);
    }
}