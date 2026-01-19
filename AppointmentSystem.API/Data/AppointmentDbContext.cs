using AppointmentSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.API.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PatientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AppointmentDate).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
        }
    }
}