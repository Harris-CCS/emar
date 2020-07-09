using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Data
{
    public class EmarContext : DbContext
    {
        private readonly DbContextOptions _options;

        public EmarContext(DbContextOptions options) : base(options)
        {
            _options = options;
        }

        public DbSet<ExternalId> ExternalIds { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderAdministration> OrderAdministrations { get; set; }
        public DbSet<OrderEvent> OrderEvents { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>().HasMany(patient => patient.Orders).WithOne().HasForeignKey(order => order.PatientId);
            modelBuilder.Entity<Order>().HasMany(order => order.Events).WithOne().HasForeignKey(@event => @event.OrderId);
            modelBuilder.Entity<Order>().HasMany(order => order.Administrations).WithOne().HasForeignKey(administration => administration.OrderId);
            modelBuilder.Entity<OrderAdministration>().HasMany(administration => administration.Events).WithOne().HasForeignKey(@event => @event.AdministrationId);
            //modelBuilder.Entity<Order>().HasOne(order => order.MedicationRouteId).WithOne().HasForeignKey<(mr => mr.)
        }
    }
}

