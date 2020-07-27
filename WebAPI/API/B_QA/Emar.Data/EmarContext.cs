using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Data
{
    public partial class EmarContext : DbContext
    {
        private readonly DbContextOptions _options;

        public EmarContext(DbContextOptions options) : base(options)
        {
            _options = options;
        }

        public virtual DbSet<Action> Actions { get; set; }
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
            modelBuilder.Entity<Patient>().HasOne(patient => patient.Site).WithMany().HasForeignKey(patient => patient.SiteId);
            modelBuilder.Entity<Patient>().HasMany(patient => patient.PatientOrders).WithOne().HasForeignKey(order => order.PatientId);
            modelBuilder.Entity<Patient>().HasMany(patient => patient.PatientCartOrders).WithOne().HasForeignKey(order => order.PatientId);
            //////modelBuilder.Entity<PatientOrder>().HasOne(order => order.Patient).WithMany(patient => patient.PatientOrders).HasForeignKey(order => order.PatientId);
            modelBuilder.Entity<PatientOrder>().HasOne(order => order.MedicationRoute).WithMany().HasForeignKey(order => order.MedicationRouteId);
            modelBuilder.Entity<PatientOrder>().HasOne(order => order.AddUser).WithMany().HasForeignKey(order => order.AddUserId);
            modelBuilder.Entity<PatientOrder>().HasOne(order => order.OrderPhysicianUser).WithMany().HasForeignKey(order => order.OrderPhysicianUserId);
            modelBuilder.Entity<PatientOrder>().HasMany(order => order.OrderAdministrations).WithOne().HasForeignKey(administration => administration.PatientOrderId);
            modelBuilder.Entity<PatientOrder>().HasMany(order => order.OrderEvents).WithOne().HasForeignKey(@event => @event.PatientOrderId);
            modelBuilder.Entity<OrderAdministration>().HasOne(administration => administration.AcknowledgeUser).WithMany().HasForeignKey(administration => administration.AcknowledgeUserId);
            modelBuilder.Entity<OrderAdministration>().HasOne(administration => administration.AdministeringUser).WithMany().HasForeignKey(administration => administration.AdministeringUserId);
            modelBuilder.Entity<OrderAdministration>().HasOne(administration => administration.StopUser).WithMany().HasForeignKey(administration => administration.StopUserId);
            modelBuilder.Entity<OrderAdministration>().HasMany(administration => administration.OrderEvents).WithOne().HasForeignKey(@event => @event.OrderAdministrationId);
            //////modelBuilder.Entity<PatientCartOrder>().HasOne(order => order.Patient).WithMany(patient => patient.PatientCartOrders).HasForeignKey(order => order.PatientId);
            modelBuilder.Entity<PatientCartOrder>().HasOne(order => order.MedicationRoute).WithMany().HasForeignKey(order => order.MedicationRouteId);
            modelBuilder.Entity<PatientCartOrder>().HasOne(order => order.User).WithMany().HasForeignKey(order => order.UserId);
            modelBuilder.Entity<PatientCartOrder>().HasMany(order => order.CartOrderAdministrations).WithOne().HasForeignKey(administration => administration.PatientCartOrderId);
            modelBuilder.Entity<User>().HasOne(user => user.Site).WithMany().HasForeignKey(user => user.SiteId);

            modelBuilder.Entity<Action>(entity =>
            {
                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Title).IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}