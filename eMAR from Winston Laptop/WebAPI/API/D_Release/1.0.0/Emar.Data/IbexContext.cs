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

        public virtual DbSet<EmarArchivedPatientsRetrieveView> EmarArchivedPatientsRetrieveViews { get; set; }
        public virtual DbSet<EmarPatientIndicatorsRetrieveView> EmarPatientIndicatorsRetrieveViews { get; set; }
        public virtual DbSet<EmarPatientsRetrieveView> EmarPatientsRetrieveViews { get; set; }
        public virtual DbSet<EmarPersonnelRetrieveView> EmarPersonnelRetrieveViews { get; set; }
        public virtual DbSet<EmarUpdateQueueMaintenance> EmarUpdateQueueMaintenances { get; set; }
        public virtual DbSet<EmarUsersRetrieveView> EmarUsersRetrieveViews { get; set; }
        public virtual DbSet<Medication> Medications { get; set; }
        public virtual DbSet<MedicationDetails> MedicationDetails { get; set; }
        public virtual DbSet<EmarMedicationAdministrations> EmarMedicationAdministrations { get; set; }
        public virtual DbSet<IbexPatient> Patients { get; set; }
        public virtual DbSet<VitalRanges> VitalRanges { get; set; }
        public virtual DbSet<VitalTypes> VitalTypes { get; set; }

        // Entities in the "EntitiesNotMapped" list in the body of the PostMan Confirmation call (for getting responses from SPs)
        public virtual DbSet<EmarPatientAllergiesRetrieveSp> EmarPatientAllergiesRetrieveSps { get; set; }

        public virtual DbSet<EmarPatientMedicationsRetrieveSp> EmarPatientMedicationsRetrieveSps { get; set; }

        public virtual DbSet<EmarUpdateQueue> EmarUpdateQueues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (Database.IsSqlServer()) modelBuilder.AddSqlFunctions();

            modelBuilder.Entity<EmarArchivedPatientsRetrieveView>(entity =>
            {
                entity.Property(e => e.ExternalId).IsFixedLength().IsUnicode(false);

                entity.Property(e => e.AccountNumber).IsUnicode(false);

                entity.Property(e => e.Age).IsUnicode(false);

                entity.Property(e => e.AgeUnits).IsUnicode(false);

                entity.Property(e => e.Complaint).IsUnicode(false);

                entity.Property(e => e.CustomNumber).IsUnicode(false);

                entity.Property(e => e.DepartmentCode).IsUnicode(false);

                entity.Property(e => e.FirstName).IsUnicode(false);

                entity.Property(e => e.Gender).IsUnicode(false);

                entity.Property(e => e.GenderSystem).IsUnicode(false);

                entity.Property(e => e.HeightInCm).IsUnicode(false);

                entity.Property(e => e.LastName).IsUnicode(false);

                entity.Property(e => e.MedicalRecordNumber).IsUnicode(false);

                entity.Property(e => e.MiddleName).IsUnicode(false);

                entity.Property(e => e.NameAlert).IsUnicode(false);

                entity.Property(e => e.NameSuffix).IsUnicode(false);

                entity.Property(e => e.PersonNumber).IsUnicode(false);

                entity.Property(e => e.RoomBedCode).IsUnicode(false);

                entity.Property(e => e.Urgency).IsUnicode(false);

                entity.Property(e => e.UrgencyColor).IsUnicode(false);

                entity.Property(e => e.VisitStartDatetime).IsUnicode(false);

                entity.Property(e => e.VsBloodPressureIndicator).IsUnicode(false);

                entity.Property(e => e.VsDatetime).IsUnicode(false);

                entity.Property(e => e.VsDiastolic).IsUnicode(false);

                entity.Property(e => e.VsEndTidal).IsUnicode(false);

                entity.Property(e => e.VsEndTidalLevel).IsUnicode(false);

                entity.Property(e => e.VsMap).IsUnicode(false);

                entity.Property(e => e.VsMapLevel).IsUnicode(false);

                entity.Property(e => e.VsOxygenSaturation).IsUnicode(false);

                entity.Property(e => e.VsOxygenSaturationIndicator).IsUnicode(false);

                entity.Property(e => e.VsPainScale).IsUnicode(false);

                entity.Property(e => e.VsPainScaleIndicator).IsUnicode(false);

                entity.Property(e => e.VsPulse).IsUnicode(false);

                entity.Property(e => e.VsPulseIndicator).IsUnicode(false);

                entity.Property(e => e.VsRespiratory).IsUnicode(false);

                entity.Property(e => e.VsRespiratoryIndicator).IsUnicode(false);

                entity.Property(e => e.VsSystolic).IsUnicode(false);

                entity.Property(e => e.VsTemperature).IsUnicode(false);

                entity.Property(e => e.VsTemperatureIndicator).IsUnicode(false);

                entity.Property(e => e.WardCode).IsUnicode(false);

                entity.Property(e => e.WeightInKg).IsUnicode(false);
            });

            modelBuilder.Entity<EmarPatientAllergiesRetrieveSp>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.PatientId).IsUnicode(false);

                entity.Property(e => e.AccountNumber).IsUnicode(false);

                entity.Property(e => e.ActionStatus).IsUnicode(false);

                entity.Property(e => e.AddDatetime).IsUnicode(false);

                entity.Property(e => e.AddUserId).IsUnicode(false);

                entity.Property(e => e.AllergyDrugId).IsUnicode(false);

                entity.Property(e => e.AlternateName).IsUnicode(false);

                entity.Property(e => e.Category).IsUnicode(false);

                entity.Property(e => e.ChangeDatetime).IsUnicode(false);

                entity.Property(e => e.ChangeUserId).IsUnicode(false);

                entity.Property(e => e.Class).IsUnicode(false);

                entity.Property(e => e.Comment).IsUnicode(false);

                entity.Property(e => e.DrugId).IsUnicode(false);

                entity.Property(e => e.InformationSource).IsUnicode(false);

                entity.Property(e => e.InternalDrugId).IsUnicode(false);

                entity.Property(e => e.Name).IsUnicode(false);

                entity.Property(e => e.Ndc).IsUnicode(false);

                entity.Property(e => e.ParentDrugId).IsUnicode(false);

                entity.Property(e => e.ParentDrugName).IsUnicode(false);

                entity.Property(e => e.PersonNumber).IsUnicode(false);

                entity.Property(e => e.Reaction).IsUnicode(false);

                entity.Property(e => e.Schedule).IsUnicode(false);

                entity.Property(e => e.Severity).IsUnicode(false);

                entity.Property(e => e.Source).IsUnicode(false);
            });
            
            modelBuilder.Entity<EmarPatientsRetrieveView>(entity =>
            {
                entity.Property(e => e.ExternalId).IsFixedLength().IsUnicode(false);

                entity.Property(e => e.AccountNumber).IsUnicode(false);

                entity.Property(e => e.Age).IsUnicode(false);

                entity.Property(e => e.AgeUnits).IsUnicode(false);

                entity.Property(e => e.Complaint).IsUnicode(false);

                entity.Property(e => e.CustomNumber).IsUnicode(false);

                entity.Property(e => e.DepartmentCode).IsUnicode(false);

                entity.Property(e => e.FirstName).IsUnicode(false);

                entity.Property(e => e.Gender).IsUnicode(false);

                entity.Property(e => e.GenderSystem).IsUnicode(false);

                entity.Property(e => e.HeightInCm).IsUnicode(false);

                entity.Property(e => e.LastName).IsUnicode(false);

                entity.Property(e => e.MedicalRecordNumber).IsUnicode(false);

                entity.Property(e => e.MiddleName).IsUnicode(false);

                entity.Property(e => e.NameSuffix).IsUnicode(false);

                entity.Property(e => e.PersonNumber).IsUnicode(false);

                entity.Property(e => e.RoomBedCode).IsUnicode(false);

                entity.Property(e => e.Urgency).IsUnicode(false);

                entity.Property(e => e.UrgencyColor).IsUnicode(false);

                entity.Property(e => e.VisitStartDatetime).IsUnicode(false);

                entity.Property(e => e.VsBloodPressureIndicator).IsUnicode(false);

                entity.Property(e => e.VsDatetime).IsUnicode(false);

                entity.Property(e => e.VsDiastolic).IsUnicode(false);

                entity.Property(e => e.VsEndTidal).IsUnicode(false);

                entity.Property(e => e.VsEndTidalLevel).IsUnicode(false);

                entity.Property(e => e.VsMap).IsUnicode(false);

                entity.Property(e => e.VsMapLevel).IsUnicode(false);

                entity.Property(e => e.VsOxygenSaturation).IsUnicode(false);

                entity.Property(e => e.VsOxygenSaturationIndicator).IsUnicode(false);

                entity.Property(e => e.VsPainScale).IsUnicode(false);

                entity.Property(e => e.VsPainScaleIndicator).IsUnicode(false);

                entity.Property(e => e.VsPulse).IsUnicode(false);

                entity.Property(e => e.VsPulseIndicator).IsUnicode(false);

                entity.Property(e => e.VsRespiratory).IsUnicode(false);

                entity.Property(e => e.VsRespiratoryIndicator).IsUnicode(false);

                entity.Property(e => e.VsSystolic).IsUnicode(false);

                entity.Property(e => e.VsTemperature).IsUnicode(false);

                entity.Property(e => e.VsTemperatureIndicator).IsUnicode(false);

                entity.Property(e => e.WardCode).IsUnicode(false);

                entity.Property(e => e.WeightInKg).IsUnicode(false);
            });

            modelBuilder.Entity<EmarPatientIndicatorsRetrieveView>(entity =>
            {
                entity.Property(e => e.ExternalId).IsFixedLength().IsUnicode(false);

                entity.Property(e => e.ExternalSiteId).IsUnicode(false);

                entity.Property(e => e.OrdinalPosition).IsUnicode(false);

                entity.Property(e => e.Code).IsUnicode(false);

                entity.Property(e => e.Type).IsUnicode(false);

                entity.Property(e => e.TypeDescription).IsUnicode(false);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.ImageName).IsUnicode(false);
            });

            modelBuilder.Entity<EmarPersonnelRetrieveView>(entity =>
            {
                entity.Property(e => e.ExternalId).IsFixedLength().IsUnicode(false);

                entity.Property(e => e.ExternalSiteId).IsUnicode(false);

                entity.Property(e => e.ExternalUserId).IsUnicode(false);

                entity.Property(e => e.RoleName).IsUnicode(false);
            });

            modelBuilder.Entity<EmarUpdateQueue>(entity =>
            {
                entity.Property(e => e.Entity).IsUnicode(false);

                entity.Property(e => e.ExternalId).IsUnicode(false);
            });

            modelBuilder.Entity<EmarUpdateQueueMaintenance>(entity =>
            {
                entity.HasNoKey();

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

            modelBuilder.Entity<Medication>(entity =>
            {
                entity.Property(e => e.Ibex)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsUnicode(false);

                entity.Property(e => e.Route)
                    .IsUnicode(false);

                entity.Property(e => e.Unit)
                    .IsUnicode(false);

                entity.Property(e => e.Dose)
                    .IsUnicode(false);

                entity.Property(e => e.MedNotes)
                    .IsUnicode(false);

                entity.Property(e => e.OrderDate)
                    .IsUnicode(false);

                entity.Property(e => e.GiveDate)
                    .IsUnicode(false);

                entity.Property(e => e.GiveSysDate)
                    .IsUnicode(false);

                entity.Property(e => e.IVType)
                    .IsUnicode(false);

                entity.Property(e => e.IVLocation)
                    .IsUnicode(false);

                entity.Property(e => e.Indication)
                    .IsUnicode(false);

                entity.Property(e => e.IVEdit)
                    .IsUnicode(false);

                entity.Property(e => e.DataSource)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<MedicationDetails>(entity =>
            {
                entity.Property(e => e.Ibex)
                    .IsUnicode(false);

                entity.Property(e => e.BrandName)
                    .IsUnicode(false);

                entity.Property(e => e.ActiveName)
                    .IsUnicode(false);

                entity.Property(e => e.DrugRoute)
                    .IsUnicode(false);

                entity.Property(e => e.DrugForm)
                    .IsUnicode(false);

                entity.Property(e => e.DrugStrength)
                    .IsUnicode(false);

                entity.Property(e => e.DrugDbType)
                    .IsUnicode(false);

                entity.Property(e => e.ActiveId)
                    .IsUnicode(false);

                entity.Property(e => e.DrugId)
                    .IsUnicode(false);

                entity.Property(e => e.PackagingId)
                    .IsUnicode(false);

                entity.Property(e => e.DrugCategoryId)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<EmarMedicationAdministrations>(entity =>
            {
                entity.Property(e => e.Ibex)
                    .IsUnicode(false);

                entity.Property(e => e.MedAdminType)
                    .IsUnicode(false);

                entity.Property(e => e.MedAdminDate)
                    .IsUnicode(false);

                entity.Property(e => e.MedAdminSysdate)
                    .IsUnicode(false);

                entity.Property(e => e.StopDate)
                    .IsUnicode(false);

                entity.Property(e => e.StopSysdate)
                    .IsUnicode(false);

                entity.Property(e => e.IvLocation)
                    .IsUnicode(false);

                entity.Property(e => e.IvType)
                    .IsUnicode(false);

                entity.Property(e => e.IvEdit)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<IbexPatient>(entity =>
            {
                entity.Property(e => e.Ibex)
                    .IsUnicode(false);

                entity.Property(e => e.AgeUnits)
                    .IsUnicode(false);

                entity.Property(e => e.Ord30)
                    .IsUnicode(false);

                entity.Property(e => e.VSDate)
                    .IsUnicode(false);

                entity.Property(e => e.VSSys)
                    .IsUnicode(false);

                entity.Property(e => e.VSDia)
                    .IsUnicode(false);

                entity.Property(e => e.VSPulse)
                    .IsUnicode(false);

                entity.Property(e => e.VSResp)
                    .IsUnicode(false);

                entity.Property(e => e.VSTemp)
                    .IsUnicode(false);

                entity.Property(e => e.VSPain)
                    .IsUnicode(false);

                entity.Property(e => e.VSO2)
                    .IsUnicode(false);

                entity.Property(e => e.VSMap)
                    .IsUnicode(false);

                entity.Property(e => e.VSEndTidal)
                    .IsUnicode(false);

                entity.Property(e => e.VSMapLevel)
                    .IsUnicode(false);

                entity.Property(e => e.VSEndTidalLevel)
                    .IsUnicode(false);

                entity.Property(e => e.Ord11)
                    .IsUnicode(false);

                entity.Property(e => e.Ord12)
                    .IsUnicode(false);

                entity.Property(e => e.Ord13)
                    .IsUnicode(false);

                entity.Property(e => e.Ord14)
                    .IsUnicode(false);

                entity.Property(e => e.Ord15)
                    .IsUnicode(false);

                entity.Property(e => e.Ord23)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VitalRanges>()
                .HasKey(v => new { v.Id, v.Site });

            modelBuilder.Entity<VitalTypes>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsUnicode(false);
            });
            //OnModelCreatingPartial(modelBuilder);
        }

        //void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}