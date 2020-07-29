using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Design;

namespace Emar.Data
{
    public class EmarContext : DbContext
    {
        public EmarContext()
        {
        }

        public EmarContext(DbContextOptions<EmarContext> options) : base(options)
        {
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<CartOrderAdministration> CartOrderAdministrations { get; set; }
        public virtual DbSet<ExternalIdEntity> ExternalIds { get; set; }
        public virtual DbSet<GroupListItem> GroupListItems { get; set; }
        public virtual DbSet<MedicationRoute> MedicationRoutes { get; set; }
        public virtual DbSet<OrderAdministration> OrderAdministrations { get; set; }
        public virtual DbSet<OrderEvent> OrderEvents { get; set; }
        public virtual DbSet<PatientCartOrder> PatientCartOrders { get; set; }
        public virtual DbSet<PatientOrder> PatientOrders { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<Site> Sites { get; set; }
        public virtual DbSet<UserQuickListItem> UserQuickListItems { get; set; }
        public virtual DbSet<User> Users { get; set; }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            if (!optionsBuilder.IsConfigured)
//            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
//                optionsBuilder.UseSqlServer("Server=HNML6S2\\SQL2016;Database=EMAR;Trusted_Connection=True;");
//            }
//        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Action>(entity =>
            {
                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Title).IsUnicode(false);
            });

            modelBuilder.Entity<CartOrderAdministration>(entity =>
            {
                entity.HasOne(d => d.PatientCartOrder)
                    .WithMany(p => p.CartOrderAdministrations)
                    .HasForeignKey(d => d.PatientCartOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__cart_order_administrations__patient_cart_orders");
            });

            modelBuilder.Entity<ExternalIdEntity>(entity =>
            {
                entity.HasKey(e => new { e.InternalId, e.Vendor, e.Entity })
                    .HasName("pk__external_ids");

                entity.HasIndex(e => new { e.InternalId, e.ExternalId, e.Vendor, e.Entity })
                    .HasName("ui__external_ids__internal_id")
                    .IsUnique();

                entity.Property(e => e.Vendor).IsUnicode(false);

                entity.Property(e => e.Entity).IsUnicode(false);

                entity.Property(e => e.ExternalId).IsUnicode(false);
            });

            modelBuilder.Entity<GroupListItem>(entity =>
            {
                entity.Property(e => e.DoseUnit).IsUnicode(false);

                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.GroupListItems)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__group_list_items__medication_routes");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.GroupListItems)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__group_list_items__sites");
            });

            modelBuilder.Entity<OrderAdministration>(entity =>
            {
                entity.HasOne(d => d.AcknowledgeUser)
                    .WithMany(p => p.OrderAdministrationsAcknowledgeUser)
                    .HasForeignKey(d => d.AcknowledgeUserId)
                    .HasConstraintName("fk__order_administrations__patient_orders__acknowledge_user_id");

                entity.HasOne(d => d.AdministeringUser)
                    .WithMany(p => p.OrderAdministrationsAdministeringUser)
                    .HasForeignKey(d => d.AdministeringUserId)
                    .HasConstraintName("fk__order_administrations__patient_orders__administering_user_id");

                entity.HasOne(d => d.PatientOrder)
                    .WithMany(p => p.OrderAdministrations)
                    .HasForeignKey(d => d.PatientOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__order_administrations__patient_orders");
            });

            modelBuilder.Entity<OrderEvent>(entity =>
            {
                entity.HasOne(d => d.Action)
                    .WithMany(p => p.OrderEvents)
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__order_events__actions");

                entity.HasOne(d => d.OrderAdministration)
                    .WithMany(p => p.OrderEvents)
                    .HasForeignKey(d => d.OrderAdministrationId)
                    .HasConstraintName("fk__order_events__order_administrations");

                entity.HasOne(d => d.PatientOrder)
                    .WithMany(p => p.OrderEvents)
                    .HasForeignKey(d => d.PatientOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__order_events__patient_orders");
            });

            modelBuilder.Entity<PatientCartOrder>(entity =>
            {
                entity.Property(e => e.DoseUnit).IsUnicode(false);

                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__patient_cart_orders__medication_routes");
            });

            //modelBuilder.Entity<PatientCartOrder>().HasOne(order => order.Patient).WithMany(patient => patient.PatientCartOrders).HasForeignKey(order => order.PatientId);
            //modelBuilder.Entity<PatientCartOrder>().HasOne(order => order.MedicationRoute).WithMany().HasForeignKey(order => order.MedicationRouteId);
            //modelBuilder.Entity<PatientCartOrder>().HasOne(order => order.User).WithMany().HasForeignKey(order => order.UserId);
            //modelBuilder.Entity<PatientCartOrder>().HasMany(order => order.CartOrderAdministrations).WithOne().HasForeignKey(administration => administration.PatientCartOrderId);

            modelBuilder.Entity<PatientOrder>(entity =>
            {
                entity.Property(e => e.DoseUnit).IsUnicode(false);

                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.Property(e => e.OrderStatus).IsUnicode(false);

                entity.HasOne(d => d.AddUser)
                    .WithMany(p => p.PatientOrdersAddUser)
                    .HasForeignKey(d => d.AddUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_orders__users");

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.PatientOrders)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__patient_orders__medication_routes");

                entity.HasOne(d => d.OrderPhysicianUser)
                    .WithMany(p => p.PatientOrdersOrderPhysicianUser)
                    .HasForeignKey(d => d.OrderingPhysicianId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_patient_orders__user__ordering_physician");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.PatientOrders)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_orders__patients");
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(e => e.AccountNumber).IsUnicode(false);

                entity.Property(e => e.AgeUnits)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ChiefComplaint).IsUnicode(false);

                entity.Property(e => e.DepartmentCode).IsUnicode(false);

                entity.Property(e => e.Gender).IsUnicode(false);

                entity.Property(e => e.MedicalRecordNumber).IsUnicode(false);

                entity.Property(e => e.RoomBedCode).IsUnicode(false);

                entity.Property(e => e.Urgency).IsUnicode(false);

                entity.Property(e => e.UrgencyColor).IsUnicode(false);

                entity.Property(e => e.VsBloodPressureIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsDiastolic)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsEndTidal).IsUnicode(false);

                entity.Property(e => e.VsEndTidalLevel)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsMap).IsUnicode(false);

                entity.Property(e => e.VsMapLevel)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsOxygenSaturation).IsUnicode(false);

                entity.Property(e => e.VsOxygenSaturationIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsPainScale)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsPainScaleIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsPulse)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsPulseIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsRespiratory)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsRespiratoryIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsSystolic)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsTemperature)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsTemperatureIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.WardCode).IsUnicode(false);

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.Patients)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patients__sites");
            });

            modelBuilder.Entity<Site>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("uc__sites__name")
                    .IsUnique();
            });

            modelBuilder.Entity<UserQuickListItem>(entity =>
            {
                entity.Property(e => e.DoseUnit).IsUnicode(false);

                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.Property(e => e.UsagesThisWeek).HasDefaultValueSql("((0))");

                entity.Property(e => e.WeeklyUsageRollingAverage).HasDefaultValueSql("((-1))");

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__user_quick_list_items__medication_routes");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__user_quick_list_items__sites");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__user_quick_list_items__user");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => new { e.LoginName, e.SiteId })
                    .HasName("ix_users__login_name_site_id");

                entity.HasIndex(e => new { e.LastName, e.FirstName, e.SiteId })
                    .HasName("ix_users__last_name_first_name_site_id");

                entity.Property(e => e.LoginName).IsUnicode(false);

                entity.Property(e => e.LoginPassword).IsUnicode(false);

                entity.Property(e => e.NameDisplayInitials).HasDefaultValueSql("((0))");

                entity.Property(e => e.OrderingOnlyPhysician).HasDefaultValueSql("((0))");

                entity.Property(e => e.Salt).IsFixedLength();

                entity.Property(e => e.Type)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__users__sites");
            });

            //OnModelCreatingPartial(modelBuilder);
        }

        //void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    /// <summary>
    /// Added this factory so that the EF Core Power Tools could figure out what Db Provider we are using
    /// Shouldn't be used anywhere else, and should remove the hard-coding before shipping
    /// </summary>
    // todo: Remove hard-coding.
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<EmarContext>
    {
        public EmarContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<EmarContext>();
            builder.UseSqlServer("Data Source = localhost\\SQL2016; Initial Catalog = EMAR; Integrated Security=true");

            return new EmarContext(builder.Options);
        }
    }
}
