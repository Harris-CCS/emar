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
        public DbSet<PatientOrder> PatientOrders { get; set; }
        public DbSet<OrderAdministration> PatientOrderAdministrations { get; set; }
        public DbSet<OrderEvent> PatientOrderEvents { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PatientCartOrder> PatientCartOrders { get; set; }
        public DbSet<CartOrderAdministration> PatientCartOrderAdministrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>().HasMany(patient => patient.PatientOrders).WithOne().HasForeignKey(order => order.PatientId);
            modelBuilder.Entity<Patient>().HasOne(patient => patient.Site).WithMany().HasForeignKey(patient => patient.SiteId);
            modelBuilder.Entity<PatientOrder>().HasMany(order => order.OrderEvents).WithOne().HasForeignKey(@event => @event.PatientOrderId);
            modelBuilder.Entity<PatientOrder>().HasMany(order => order.OrderAdministrations).WithOne().HasForeignKey(administration => administration.PatientOrderId);
            modelBuilder.Entity<OrderAdministration>().HasMany(administration => administration.OrderEvents).WithOne().HasForeignKey(@event => @event.OrderAdministrationId);
            modelBuilder.Entity<User>().HasOne(user => user.Site).WithMany().HasForeignKey(user => user.SiteId);
            modelBuilder.Entity<PatientCartOrder>().HasMany(order => order.CartOrderAdministrations).WithOne().HasForeignKey(administration => administration.PatientCartOrderId);
        }
    }
}