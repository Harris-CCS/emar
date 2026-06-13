using Emar.Data.IbexEntities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Data
{
    public class IbexContext : DbContext
    {
        public IbexContext()
        {
        }

        public IbexContext(DbContextOptions<IbexContext> options) : base(options)
        {
            // Since EMAR is only ever retrieving data from Ibex, not writing to it,
            // we can go with the NoTracking behavior
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public virtual DbSet<EmarUpdateQueue> EmarUpdateQueues { get; set; }
        public virtual DbSet<EmarPatientsRetrieveView> EmarPatientsRetrieveViews { get; set; }
        public virtual DbSet<EmarUpdateQueueMaintenance> EmarUpdateQueueMaintenances { get; set; }
        public virtual DbSet<EmarUsersRetrieveView> EmarUsersRetrieveViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (Database.IsSqlServer()) modelBuilder.AddSqlFunctions();

            modelBuilder.Entity<EmarUpdateQueue>(entity =>
            {
                entity.Property(e => e.Entity).IsUnicode(false);

                entity.Property(e => e.ExternalId).IsUnicode(false);
            });

            modelBuilder.Entity<EmarUsersRetrieveView>(entity =>
            {
                entity.Property(e => e.FirstName).IsUnicode(false);

                entity.Property(e => e.InitialsDisplay).IsUnicode(false);

                entity.Property(e => e.LastLoginTime).IsUnicode(false);

                entity.Property(e => e.LastName).IsUnicode(false);

                entity.Property(e => e.LoginName).IsUnicode(false);

                entity.Property(e => e.LoginPassword).IsUnicode(false);

                entity.Property(e => e.MiddleName).IsUnicode(false);

                entity.Property(e => e.NameSuffix).IsUnicode(false);

                entity.Property(e => e.Type).IsUnicode(false);

                entity.Property(e => e.MedicationServicesAccess).IsUnicode(false);
            });

            modelBuilder.Entity<EmarUpdateQueueMaintenance>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Entity).IsUnicode(false);

                entity.Property(e => e.ExternalId).IsUnicode(false);

            }); 

            //OnModelCreatingPartial(modelBuilder);
        }

        //void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}