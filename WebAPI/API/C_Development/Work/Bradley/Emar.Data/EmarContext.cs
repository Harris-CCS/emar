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

        public virtual DbSet<Entities.Action> Actions { get; set; }
        public virtual DbSet<AllergyReactionView> AllergyReactionsView { get; set; }
        public virtual DbSet<CartOrderAdministration> CartOrderAdministrations { get; set; }
        public virtual DbSet<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }
        public virtual DbSet<DrugInteractionView> DrugInteractionsView { get; set; }
        public virtual DbSet<ExternalIdEntity> ExternalIds { get; set; }
        public virtual DbSet<FdbAllergyName> FdbAllergyName { get; set; }
        public virtual DbSet<FdbBrandName> FdbBrandName { get; set; }
        public virtual DbSet<FdbNdcInfo> FdbNdcInfo { get; set; }
        public virtual DbSet<FrequencySchedule> FrequencySchedules { get; set; }
        public virtual DbSet<FrequencyScheduleAdministration> FrequencyScheduleAdministrations { get; set; }
        public virtual DbSet<GroupListItem> GroupListItems { get; set; }
        public virtual DbSet<MedicationInteraction> MedicationInteractions { get; set; }
        public virtual DbSet<MedicationDetail> MedicationDetails { get; set; }
        public virtual DbSet<MedicationRoute> MedicationRoutes { get; set; }
        public virtual DbSet<Medication> Medications { get; set; }
        public virtual DbSet<MedicationUnit> MedicationUnits { get; set; }
        public virtual DbSet<Option> Options { get; set; }
        public virtual DbSet<OrderAdministration> OrderAdministrations { get; set; }
        public virtual DbSet<OrderEvent> OrderEvents { get; set; }
        public virtual DbSet<OrderInteraction> OrderInteractions { get; set; }
        public virtual DbSet<OrderReaction> OrderReactions { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<PatientAllergy> PatientAllergies { get; set; }
        public virtual DbSet<PatientCartOrder> PatientCartOrders { get; set; }
        public virtual DbSet<PatientHomeMedication> PatientHomeMedications { get; set; }
        public virtual DbSet<PatientIndicator> PatientIndicators { get; set; }
        public virtual DbSet<PatientOrder> PatientOrders { get; set; }
        public virtual DbSet<Prompt> Prompts { get; set; }
        public virtual DbSet<PromptChoice> PromptChoices { get; set; }
        public virtual DbSet<PromptGroup> PromptGroups { get; set; }
        public virtual DbSet<Site> Sites { get; set; }
        public virtual DbSet<SiteOption> SiteOptions { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<TemplatePromptGroup> TemplatePromptGroups { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserQuickListItem> UserQuickListItems { get; set; }

        //SP entities
        public virtual DbSet<DoseRangeCheckingInfo> DoseRangeCheckingInfos { get; set; }

        // Testing Code
#if  TestingEfUtility
#endif
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Testing Code
#if TestingEfUtility
#endif
            modelBuilder.Entity<Entities.Action>(entity =>
            {
                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Title).IsUnicode(false);
            });

            modelBuilder.Entity<AllergyReactionView>(entity =>
            {
                entity.ToView("allergy_reactions_view");

                entity.Property(e => e.PatientAllergyName).IsUnicode(false);

                entity.Property(e => e.OrderTable).IsUnicode(false);

                entity.HasOne(d => d.OverrideReason)
                    .WithMany(p => p.AllergyReactionsView)
                    .HasForeignKey(d => d.OverrideReasonId);

                entity.HasOne(d => d.OverrideReasonUser)
                    .WithMany(p => p.AllergyReactionsView)
                    .HasForeignKey(d => d.OverrideReasonUserId);
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

                entity.HasOne(d => d.FrequencySchedule)
                    .WithMany(p => p.DepartmentPreferredListItems)
                    .HasForeignKey(d => d.FrequencyScheduleId)
                    .HasConstraintName("fk__department_preferred_list_items__frequency_schedules");

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.DepartmentPreferredListItems)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__department_preferred_list_items__medications");

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

            modelBuilder.Entity<DoseRangeCheckingInfo>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.AgeDdescription).IsUnicode(false);

                entity.Property(e => e.AmountHigh).IsUnicode(false);

                entity.Property(e => e.AmountLow).IsUnicode(false);

                entity.Property(e => e.Condition1Description).IsUnicode(false);

                entity.Property(e => e.MaxFrequency).IsUnicode(false);

                entity.Property(e => e.RenalDescription).IsUnicode(false);

                entity.Property(e => e.RouteDescription).IsUnicode(false);

                entity.Property(e => e.TypeDescription).IsUnicode(false);

                entity.Property(e => e.UnitDoseAbbreviation).IsUnicode(false);

                entity.Property(e => e.WeightDescription).IsUnicode(false);
            });

            modelBuilder.Entity<DrugInteractionView>(entity =>
            {
                entity.ToView("drug_interactions_view");

                entity.Property(e => e.InteractionDrug1).IsUnicode(false);

                entity.Property(e => e.InteractionDrug2).IsUnicode(false);

                entity.Property(e => e.OrderTable1).IsUnicode(false);

                entity.Property(e => e.OrderTable2).IsUnicode(false);

                entity.HasOne(d => d.OverrideReason)
                    .WithMany(p => p.DrugInteractionsView)
                    .HasForeignKey(d => d.OverrideReasonId);

                entity.HasOne(d => d.OverrideReasonUser)
                    .WithMany(p => p.DrugInteractionsView)
                    .HasForeignKey(d => d.OverrideReasonUserId);
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

            modelBuilder.Entity<FdbAllergyName>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.AllergyName)
                    .HasName("NonClusteredIndex-20140611-103253");

                entity.HasIndex(e => e.MedName)
                    .HasName("NonClusteredIndex-20140611-103242");

                entity.HasIndex(e => e.MedNameId)
                    .HasName("NonClusteredIndex-20140611-102020");

                entity.HasIndex(e => e.Medid)
                    .HasName("ClusteredIndex-20140611-084822")
                    .IsClustered();

                entity.Property(e => e.AllergyName)
                    .IsUnicode(false);

                entity.Property(e => e.MedName)
                    .IsUnicode(false);

                entity.Property(e => e.PcHiclSeqno)
                    .IsUnicode(false);

                entity.Property(e => e.PcMedNameId)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FdbBrandName>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.BrandName)
                    .HasName("NonClusteredIndex-20140611-101716");

                entity.HasIndex(e => e.Medid)
                    .HasName("ClusteredIndex-20140611-085119")
                    .IsClustered();

                entity.HasIndex(e => e.PcRoutedGenId)
                    .HasName("NonClusteredIndex-20140611-101732");

                entity.Property(e => e.Active)
                    .IsUnicode(false);

                entity.Property(e => e.BrandName)
                    .IsUnicode(false);

                entity.Property(e => e.DeaSchedule)
                    .IsUnicode(false);

                entity.Property(e => e.LongBrandName)
                    .IsUnicode(false);

                entity.Property(e => e.PcMedNameId)
                    .IsUnicode(false);

                entity.Property(e => e.PcRoutedGenId)
                    .IsUnicode(false);

                entity.Property(e => e.RxOtc)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FdbNdcInfo>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.Ndc)
                    .HasName("ndc");

                entity.HasIndex(e => new { e.Ndc, e.BaseNdc })
                    .HasName("ndc-base_ndc")
                    .IsClustered();

                entity.Property(e => e.BaseNdc)
                    .IsUnicode(false);

                entity.Property(e => e.Ndc)
                    .IsUnicode(false);

                entity.Property(e => e.Packaging)
                    .IsUnicode(false);

                entity.Property(e => e.Strength)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FrequencySchedule>(entity =>
            {
                entity.HasIndex(e => new { e.Name, e.SiteId })
                    .HasName("uk__frequency_schedules__name_site_id")
                    .IsUnique();

                entity.Property(e => e.IsActive)
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.FrequencySchedules)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__frequency_schedules__sites");
            });

            modelBuilder.Entity<FrequencyScheduleAdministration>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<GroupListItem>(entity =>
            {
                entity.Property(e => e.DepartmentCode).IsUnicode(false);

                entity.HasOne(d => d.FrequencySchedule)
                    .WithMany(p => p.GroupListItems)
                    .HasForeignKey(d => d.FrequencyScheduleId)
                    .HasConstraintName("fk__group_list_items__frequency_schedules");

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.GroupListItems)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__group_list_items__medications");

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

            modelBuilder.Entity<MedicationInteraction>(entity =>
            {
                entity.Property(e => e.InteractionDrug1).IsUnicode(false);

                entity.Property(e => e.InteractionDrug2).IsUnicode(false);

                entity.HasOne(d => d.OverrideReason)
                    .WithMany(p => p.MedicationInteractions)
                    .HasForeignKey(d => d.OverrideReasonId)
                    .HasConstraintName("fk__medication_interactions__override_reasons");

                entity.HasOne(d => d.OverrideReasonUser)
                    .WithMany(p => p.MedicationInteractions)
                    .HasForeignKey(d => d.OverrideReasonUserId)
                    .HasConstraintName("fk__medication_interactions__users");
            });

            modelBuilder.Entity<Medication>(entity =>
            {
                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.DrugVendor)
                    .IsFixedLength()
                    .IsUnicode(false);

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.Medications)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__medications__sites");
            });

            modelBuilder.Entity<MedicationRoute>(entity =>
            {
                entity.HasOne(d => d.Site)
                    .WithMany(p => p.MedicationRoutes)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__medication_routes__sites");
            });

            modelBuilder.Entity<MedicationDetail>(entity =>
            {
                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.MedicationDetails)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__medication_details__medications");
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__order_events__order_administrations");

                entity.HasOne(d => d.PatientOrder)
                    .WithMany(p => p.OrderEvents)
                    .HasForeignKey(d => d.PatientOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__order_events__patient_orders");
            });

            modelBuilder.Entity<OrderInteraction>(entity =>
            {
                entity.HasOne(d => d.DrugInteractionView)
                    .WithMany(p => p.OrderInteractions)
                    .HasForeignKey(d => d.MedicationInteractionId);

                entity.HasOne(d => d.MedicationInteraction)
                    .WithMany(p => p.OrderInteractions)
                    .HasForeignKey(d => d.MedicationInteractionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__order_interactions__medication_interactions");

                entity.HasOne(d => d.PatientCartOrder)
                    .WithMany(p => p.OrderInteractions)
                    .HasForeignKey(d => d.PatientCartOrderId)
                    .HasConstraintName("fk__order_interactions__patient_cart_orders");

                entity.HasOne(d => d.PatientHomeMedication)
                    .WithMany(p => p.OrderInteractions)
                    .HasForeignKey(d => d.PatientHomeMedicationId)
                    .HasConstraintName("fk__order_interactions__patient_home_medications");

                entity.HasOne(d => d.PatientOrder)
                    .WithMany(p => p.OrderInteractions)
                    .HasForeignKey(d => d.PatientOrderId)
                    .HasConstraintName("fk__order_interactions__patient_orders");
            });

            modelBuilder.Entity<OrderReaction>(entity =>
            {
                entity.HasOne(d => d.OverrideReason)
                    .WithMany(p => p.OrderReactions)
                    .HasForeignKey(d => d.OverrideReasonId)
                    .HasConstraintName("FK_order_reactions_override_reasons");

                entity.HasOne(d => d.OverrideReasonUser)
                    .WithMany(p => p.OrderReactions)
                    .HasForeignKey(d => d.OverrideReasonUserId)
                    .HasConstraintName("FK_order_reactions_users");

                entity.HasOne(d => d.PatientAllergy)
                    .WithMany(p => p.OrderReactions)
                    .HasForeignKey(d => d.PatientAllergyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_reactions_patient_allergies");

                entity.HasOne(d => d.PatientCartOrder)
                    .WithMany(p => p.OrderReactions)
                    .HasForeignKey(d => d.PatientCartOrderId)
                    .HasConstraintName("FK_order_reactions_patient_cart_orders");

                entity.HasOne(d => d.PatientOrder)
                    .WithMany(p => p.OrderReactions)
                    .HasForeignKey(d => d.PatientOrderId)
                    .HasConstraintName("FK_order_reactions_patient_orders");
            });

            modelBuilder.Entity<OverrideReason>(entity =>
            {
                entity.Property(e => e.Description).IsUnicode(false);

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.OverrideReasons)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__override_reasons__sites");
            });

            modelBuilder.Entity<PatientAllergy>(entity =>
            {
                entity.Property(e => e.AccountNumber)
                    .IsUnicode(false);

                entity.Property(e => e.ActionStatus)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.AllergyDrugId)
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .IsUnicode(false);

                entity.Property(e => e.Class)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .IsUnicode(false);

                entity.Property(e => e.InformationSource)
                    .IsUnicode(false);

                entity.Property(e => e.InternalDrugId)
                    .IsUnicode(false);

                entity.Property(e => e.ParentDrugId)
                    .IsUnicode(false);

                entity.Property(e => e.PersonNumber)
                    .IsUnicode(false);

                entity.Property(e => e.Reaction)
                    .IsUnicode(false);

                entity.Property(e => e.Schedule)
                    .IsUnicode(false);

                entity.Property(e => e.Severity)
                    .IsUnicode(false);

                entity.HasOne(d => d.AddUser)
                    .WithMany(p => p.PatientAllergiesAddUser)
                    .HasForeignKey(d => d.AddUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__users__patient_allergies__add_user_id");

                entity.HasOne(d => d.ChangeUser)
                    .WithMany(p => p.PatientAllergiesChangeUser)
                    .HasForeignKey(d => d.ChangeUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__users__patient_allergies__change_user_id");

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.PatientAllergys)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_allergies__medications");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.PatientAllergies)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patients__patient_allergies");
            });

            modelBuilder.Entity<PatientCartOrder>(entity =>
            {
                entity.HasOne(d => d.FrequencySchedule)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.FrequencyScheduleId)
                    .HasConstraintName("fk__patient_cart_orders__frequency_schedules");

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.PatientCartOrders)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_cart_orders__medications");

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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_cart_orders__user_quick_list_items");
            });

            modelBuilder.Entity<PatientHomeMedication>(entity =>
            {
                entity.Property(e => e.Category)
                    .IsUnicode(false);

                entity.Property(e => e.Class)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .IsUnicode(false);

                entity.Property(e => e.InternalDrugId)
                    .IsUnicode(false);

                entity.Property(e => e.MedicationDrugId)
                    .IsUnicode(false);

                entity.Property(e => e.ParentDrugId)
                    .IsUnicode(false);

                entity.Property(e => e.Reaction)
                    .IsUnicode(false);

                entity.Property(e => e.Schedule)
                    .IsUnicode(false);

                entity.Property(e => e.Severity)
                    .IsUnicode(false);

                entity.Property(e => e.ActionStatus)
                    .IsFixedLength()
                    .IsUnicode(false);

                entity.HasOne(d => d.AddUser)
                    .WithMany(p => p.PatientHomeMedicationsAddUser)
                    .HasForeignKey(d => d.AddUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__users__patient_home_medications__add_user_id");

                entity.HasOne(d => d.ChangeUser)
                    .WithMany(p => p.PatientHomeMedicationsChangeUser)
                    .HasForeignKey(d => d.ChangeUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__users__patient_home_medications__change_user_id");

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.PatientHomeMedications)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_home_medications__medications");

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.PatientHomeMedications)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__patient_home_medications__medication_routes");

                entity.HasOne(d => d.MedicationUnit)
                    .WithMany(p => p.PatientHomeMedications)
                    .HasForeignKey(d => d.MedicationUnitId)
                    .HasConstraintName("fk__patient_home_medications__medication_units");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.PatientHomeMedications)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patients__patient_home_medications");
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
                entity.Property(e => e.OrderStatus).IsUnicode(false);

                entity.HasOne(d => d.AddUser)
                    .WithMany(p => p.PatientOrdersAddUser)
                    .HasForeignKey(d => d.AddUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_orders__users__add_user_id");

                entity.HasOne(d => d.FrequencySchedule)
                    .WithMany(p => p.PatientOrders)
                    .HasForeignKey(d => d.FrequencyScheduleId)
                    .HasConstraintName("fk__patient_orders__frequency_schedules");

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.PatientOrders)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__patient_orders__medications");

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
                entity.Property(e => e.AccountNumber).IsUnicode(false);

                entity.Property(e => e.AgeUnits)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Complaint).IsUnicode(false);

                entity.Property(e => e.CustomNumber).IsUnicode(false);

                entity.Property(e => e.DepartmentCode).IsUnicode(false);

                entity.Property(e => e.Gender).IsUnicode(false);

                entity.Property(e => e.MedicalRecordNumber).IsUnicode(false);

                entity.Property(e => e.PersonNumber).IsUnicode(false);

                entity.Property(e => e.RoomBedCode).IsUnicode(false);

                entity.Property(e => e.Urgency).IsUnicode(false);

                entity.Property(e => e.UrgencyColor).IsUnicode(false);

                entity.Property(e => e.VsBloodPressureIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsDiastolic).IsUnicode(false);

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

                entity.Property(e => e.VsPainScale).IsUnicode(false);

                entity.Property(e => e.VsPainScaleIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsPulse).IsUnicode(false);

                entity.Property(e => e.VsPulseIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsRespiratory).IsUnicode(false);

                entity.Property(e => e.VsRespiratoryIndicator)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.VsSystolic).IsUnicode(false);

                entity.Property(e => e.VsTemperature).IsUnicode(false);

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

            modelBuilder.Entity<Prompt>(entity =>
            {
                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.PromptDefault).IsUnicode(false);

                entity.Property(e => e.PromptType).IsUnicode(false);

                entity.HasOne(d => d.PromptGroup)
                    .WithMany(p => p.Prompts)
                    .HasForeignKey(d => d.PromptGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__prompts__prompt_groups");
            });

            modelBuilder.Entity<PromptChoice>(entity =>
            {
                entity.Property(e => e.ChoiceText).IsUnicode(false);

                entity.HasOne(d => d.Prompt)
                    .WithMany(p => p.PromptChoices)
                    .HasForeignKey(d => d.PromptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__prompt_choices__prompts");
            });

            modelBuilder.Entity<PromptGroup>(entity =>
            {
                entity.Property(e => e.Name).IsUnicode(false);

                entity.Property(e => e.Title).IsUnicode(false);
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

            modelBuilder.Entity<Template>(entity =>
            {
                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.Title).IsUnicode(false);
            });

            modelBuilder.Entity<TemplatePromptGroup>(entity =>
            {
                //entity.HasKey(t => new {t.TemplateId, t.PromptGroupId});

                entity.HasOne(d => d.PromptGroup)
                    .WithMany(p => p.TemplatePromptGroups)
                    .HasForeignKey(d => d.PromptGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__template_prompt_groups__prompt_groups");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.TemplatePromptGroups)
                    .HasForeignKey(d => d.TemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__template_prompt_groups__templates");
            });

            modelBuilder.Entity<UserQuickListItem>(entity =>
            {
                entity.Property(e => e.UsagesThisWeek).HasDefaultValueSql("((0))");

                entity.Property(e => e.WeeklyUsageRollingAverage).HasDefaultValueSql("((-1))");

                entity.HasOne(d => d.FrequencySchedule)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.FrequencyScheduleId)
                    .HasConstraintName("fk__user_quick_list_items__frequency_schedules");

                entity.HasOne(d => d.Medication)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.MedicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk__user_quick_list_items__medications");

                entity.HasOne(d => d.MedicationRoute)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.MedicationRouteId)
                    .HasConstraintName("fk__user_quick_list_items__medication_routes");

                entity.HasOne(d => d.MedicationUnit)
                    .WithMany(p => p.UserQuickListItems)
                    .HasForeignKey(d => d.MedicationUnitId)
                    .HasConstraintName("fk__user_quick_list_items__medication_units");

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
