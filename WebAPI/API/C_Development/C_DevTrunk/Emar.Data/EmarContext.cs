using System.IO;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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

        public virtual DbSet<PatientIndicator> PatientIndicators { get; set; }
        public virtual DbSet<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }
        public virtual DbSet<Entities.Action> Actions { get; set; }
        public virtual DbSet<CartOrderAdministration> CartOrderAdministrations { get; set; }
        public virtual DbSet<ExternalIdEntity> ExternalIds { get; set; }
        public virtual DbSet<GroupListItem> GroupListItems { get; set; }
        public virtual DbSet<MedicationRoute> MedicationRoutes { get; set; }
        public virtual DbSet<MedicationUnit> MedicationUnits { get; set; }
        public virtual DbSet<Option> Options { get; set; }
        public virtual DbSet<OrderAdministration> OrderAdministrations { get; set; }
        public virtual DbSet<OrderEvent> OrderEvents { get; set; }
        public virtual DbSet<PatientCartOrder> PatientCartOrders { get; set; }
        public virtual DbSet<PatientOrder> PatientOrders { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<Site> Sites { get; set; }
        public virtual DbSet<SiteOption> SiteOptions { get; set; }
        public virtual DbSet<UserQuickListItem> UserQuickListItems { get; set; }
        public virtual DbSet<User> Users { get; set; }

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

            modelBuilder.Entity<DepartmentPreferredListItem>(entity =>
            {
                entity.Property(e => e.DepartmentCode).IsUnicode(false);

                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.Property(e => e.BrandName).IsUnicode(true);



                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.DepartmentPreferredListItems)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__department_preferred_list_items__medication_routes");

                entity.HasOne(d => d.MedicationUnit)
                    .WithMany(p => p.DepartmentPreferredListItems)
                    .HasForeignKey(d => d.MedicationUnitId)
                    .HasConstraintName("fk__department_preferred_list_items__medication_units");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.DepartmentPreferredListItems)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__department_preferred_list_items__sites");
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
                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.Property(e => e.DepartmentCode).IsUnicode(false);

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.GroupListItems)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__group_list_items__medication_routes");

                entity.HasOne(d => d.MedicationUnit)
                    .WithMany(p => p.GroupListItems)
                    .HasForeignKey(d => d.MedicationUnitId)
                    .HasConstraintName("fk__group_list_items__medication_units");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.GroupListItems)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__group_list_items__sites");
            });

            modelBuilder.Entity<MedicationRoute>(entity =>
            {
                entity.HasOne(d => d.Site)
                    .WithMany(p => p.MedicationRoutes)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__medication_routes__sites");
            });

            modelBuilder.Entity<MedicationUnit>(entity =>
            {
                entity.Property(e => e.Code).IsUnicode(false);

                entity.Property(e => e.Name).IsUnicode(false);

                entity.Property(e => e.PrintName).IsUnicode(false);

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.MedicationUnits)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__medication_units__sites");
            });

            modelBuilder.Entity<Option>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrderAdministration>(entity =>
            {
                entity.HasOne(d => d.AcknowledgeUser)
                    .WithMany(p => p.OrderAdministrationsAcknowledgeUser)
                    .HasForeignKey(d => d.AcknowledgeUserId)
                    .HasConstraintName("fk__order_administrations__patient_orders__acknowledge_user_id");

                entity.HasOne(d => d.AdministeringUser)
                    .WithMany(p => p.OrderAdministrationAdministeringUser)
                    .HasForeignKey(d => d.AdministeringUserId)
                    .HasConstraintName("fk__order_administrations__patient_orders__administering_user_id");

                entity.HasOne(d => d.PatientOrder)
                    .WithMany(p => p.OrderAdministrations)
                    .HasForeignKey(d => d.PatientOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__order_administrations__patient_orders");

                entity.HasOne(d => d.StopUser)
                    .WithMany(p => p.OrderAdministrationStopUser)
                    .HasForeignKey(d => d.StopUserId)
                    .HasConstraintName("fk__order_administrations__patient_orders__stop_user_id");
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
                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__patient_cart_orders__medication_routes");

                entity.HasOne(d => d.MedicationUnit)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.MedicationUnitId)
                    .HasConstraintName("fk__patient_cart_orders__medication_units");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_cart_orders__patients");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_cart_orders__users");

                entity.HasOne(d => d.UserQuickListItem)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.UserQuickListItemId)
                    .HasConstraintName("fk__patient_cart_orders__user_quick_list_items");
            });

            modelBuilder.Entity<PatientIndicator>(entity =>
            {
                entity.HasIndex(e => e.PatientId)
                    .HasName("ix__patient_indicators__patient_id_site_id");

                entity.Property(e => e.Code)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsUnicode(false);

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.PatientIndicators)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_indicators__patients");
            });

            modelBuilder.Entity<PatientOrder>(entity =>
            {
                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.Property(e => e.OrderStatus).IsUnicode(false);

                entity.HasOne(d => d.AddUser)
                    .WithMany(p => p.PatientOrdersAddUser)
                    .HasForeignKey(d => d.AddUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_orders__users__add_user_id");

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.PatientOrders)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__patient_orders__medication_routes");

                entity.HasOne(d => d.MedicationUnit)
                    .WithMany(p => p.PatientOrders)
                    .HasForeignKey(d => d.MedicationUnitId)
                    .HasConstraintName("fk__patient_orders__medication_units");

                entity.HasOne(d => d.OrderPhysicianUser)
                    .WithMany(p => p.PatientOrdersOrderPhysicianUser)
                    .HasForeignKey(d => d.OrderingPhysicianId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_orders__users__order_physician_user_id");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.PatientOrders)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_orders__patients");
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(e => e.AccountNumber)
                    .IsUnicode(false);

                entity.Property(e => e.AgeUnits)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Complaint)
                    .IsUnicode(false);

                entity.Property(e => e.CustomNumber)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentCode)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .IsUnicode(false);

                entity.Property(e => e.MedicalRecordNumber)
                    .IsUnicode(false);

                entity.Property(e => e.PersonNumber)
                    .IsUnicode(false);

                entity.Property(e => e.RoomBedCode)
                    .IsUnicode(false);

                entity.Property(e => e.Urgency)
                    .IsUnicode(false);

                entity.Property(e => e.UrgencyColor)
                    .IsUnicode(false);

                entity.Property(e => e.VsBloodPressureIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsDiastolic)
                    .IsUnicode(false);

                entity.Property(e => e.VsEndTidal)
                    .IsUnicode(false);

                entity.Property(e => e.VsEndTidalLevel)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsMap)
                    .IsUnicode(false);

                entity.Property(e => e.VsMapLevel)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsOxygenSaturation)
                    .IsUnicode(false);

                entity.Property(e => e.VsOxygenSaturationIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsPainScale)
                    .IsUnicode(false);

                entity.Property(e => e.VsPainScaleIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsPulse)
                    .IsUnicode(false);

                entity.Property(e => e.VsPulseIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsRespiratory)
                    .IsUnicode(false);

                entity.Property(e => e.VsRespiratoryIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsSystolic)
                    .IsUnicode(false);

                entity.Property(e => e.VsTemperature)
                    .IsUnicode(false);

                entity.Property(e => e.VsTemperatureIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.WardCode)
                    .IsUnicode(false);

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

            modelBuilder.Entity<SiteOption>(entity =>
            {
                entity.HasIndex(e => new { e.OptionValue, e.OptionId, e.SiteId })
                    .HasName("site_options__option_id_site_id");

                entity.Property(e => e.OptionValue)
                    .IsUnicode(false);

                entity.HasOne(d => d.Option)
                    .WithMany(p => p.SiteOptions)
                    .HasForeignKey(d => d.OptionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__site_options__options");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.SiteOptions)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__site_options__sites");
            });

            modelBuilder.Entity<UserQuickListItem>(entity =>
            {
                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.Property(e => e.UsagesThisWeek).HasDefaultValueSql("((0))");

                entity.Property(e => e.WeeklyUsageRollingAverage).HasDefaultValueSql("((-1))");

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__user_quick_list_items__medication_routes");

                entity.HasOne(d => d.MedicationUnit)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.MedicationUnitId)
                    .HasConstraintName("FK_user_quick_list_items__medication_unit");

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

                entity.Property(e => e.DisplayInitialsIndicator).HasDefaultValueSql("((0))");

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
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<EmarContext>
    {
        public EmarContext CreateDbContext(string[] args)
        {
            var jsonFilename = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"appsettings.development.json")) ? @"appsettings.development.json" : @"appsettings.json";

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory().Replace(@"Emar.Data", @"Emar.Api"))
                .AddJsonFile(jsonFilename)
                .Build();

            var builder = new DbContextOptionsBuilder<EmarContext>();

            builder.UseSqlServer(ConfigurationExtensions.GetConnectionString(configuration, @"SqlConnection"));
            return new EmarContext(builder.Options);
        }
    }
}
