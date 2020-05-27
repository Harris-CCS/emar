using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeFirstStoreFunctions;
using PulseCheck.Data.Repositories;
using PulseCheck.Domain;
using PulseCheck.IData;
using PulseCheck.Utilities;

namespace PulseCheck.Data
{

    public class AllergyResultType
    {
        public Int32 Num { get; set; }
        public byte SiteId { get; set; }
        public string Ibex { get; set; }
        public string Name { get; set; }
        public string ReactionCode { get; set; }
        public string ReactionName { get; set; }
        public string SeverityCode { get; set; }
        public string SeverityName { get; set; }
        public string SourceCode { get; set; }
        public string SourceName { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public string ActionStatus { get; set; }
        public int User { get; set; }
        public string UserInit { get; set; }
        public int UserChg { get; set; }
        public string UserChgInit { get; set; }
        public string DateAdd { get; set; }
        public string DateChg { get; set; }
    }

    public class ServiceResultType
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public int SvcType { get; set; }
        public string Face { get; set; }
        public int MaxQty { get; set; }
        public int Number { get; set; }
        public bool IsUserFavorite { get; set; }
    }

    public class ClinicalPathwayResultType
    {
        public string Name { get; set; }
        public int Num { get; set; }
        public string Status { get; set; }
    }

    public class CurrentMedicationResultType
    {
        public Int32 Num { get; set; }
        public byte SiteId { get; set; }
        public string Ibex { get; set; }
        public string Name { get; set; }
        public string Dose { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string RouteCode { get; set; }
        public string RouteName { get; set; }
        public string ScheduleCode { get; set; }
        public string ScheduleName { get; set; }
        public string LastTaken { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public string ActionStatus { get; set; }
        public int User { get; set; }
        public string UserInit { get; set; }
        public int UserChg { get; set; }
        public string UserChgInit { get; set; }
        public string DateAdd { get; set; }
        public string DateChg { get; set; }
    }

    public class CommentResultType
    {
        public byte SiteId { get; set; }
        public Int32 CommentNum { get; set; }
        public string Ibex { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public int User { get; set; }
        public string UserInit { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public string ColorVal1 { get; set; }
        public string ColorVal2 { get; set; }
        public int Losecs { get; set; }
    }

    public class ElementResultType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public string ColorVal1 { get; set; }
        public string ColorVal2 { get; set; }
    }

    public class EncounterResultType : IResultTypeWithProviders
    {
        public byte SiteId { get; set; }
        public string SiteName { get; set; }
        public string Ibex { get; set; }
        public string AcctNum { get; set; }
        public string Complaint { get; set; }
        public string ComplaintCode { get; set; }
        public string Diagnosis { get; set; }
        public string DispoCode { get; set; }
        public string DispoLoc { get; set; }
        public string DispoCodeName { get; set; }
        public string DispoLocName { get; set; }
        public int Doctor { get; set; }
        public string DoctorInit { get; set; }
        public int FirstDoctor { get; set; }
        public string FirstDoctorInit { get; set; }
        public int Resident { get; set; }
        public string ResidentInit { get; set; }
        public int Extender { get; set; }
        public string ExtenderInit { get; set; }
        public int DrExtender { get; set; }
        public string DrExtenderInit { get; set; }
        public int PrimaryNurse { get; set; }
        public string PrimaryNurseInit { get; set; }
        public int CareCoordinator { get; set; }
        public string CareCoordinatorInit { get; set; }
        public int Scribe { get; set; }
        public string ScribeInit { get; set; }
        public int FirstResident { get; set; }
        public string FirstResidentInit { get; set; }
        public int FirstDrExtender { get; set; }
        public string FirstDrExtenderInit { get; set; }
    }

    public class FavoriteResultType
    {
        public string Name { get; set; }
        public int Num { get; set; }
        public string Status { get; set; }
    }

    public class GroupResultType
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int Num { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public string ColorVal1 { get; set; }
        public string ColorVal2 { get; set; }
        public string AltCode { get; set; }
    }

    public class LateResultResultType
    {
        public int Losecs { get; set; }
        public string AlienKey { get; set; }
        public string OrdName { get; set; }
        public string SvcName { get; set; }
        public string DteOrder { get; set; }
        public int Timeout { get; set; }
        public int UsrOrder { get; set; }
        public string UsrOrderInit { get; set; }
        public int OrderNumber { get; set; }
    }

    public class LocationResultType
    {
        public byte GroupNum { get; set; }
        public string GroupType { get; set; }
        public string Dept { get; set; }
        public string Ward { get; set; }
        public string Bed { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ibex { get; set; }
        public string LName { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string Suffix { get; set; }
    }

    public class MedMetaDataResultType
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Misc2 { get; set; }
    }

    public class OrderResultType 
    {
        public int Id { get; set; }
        public int Losecs { get; set; }
        public string Name { get; set; }
        public string ServiceCode { get; set; }
        public string StatusCode { get; set; }
        public int OrderingPhysician { get; set; }
        public int Orderer { get; set; }
        public string OrdererInit { get; set; }
        public string OrderDate { get; set; }
        public string Type { get; set; }
    }

    public class PatientResultType : IResultTypeWithProviders
    {
        public byte SiteId { get; set; }
        public string EnterpriseId { get; set; }
        public string MedRec { get; set; }
        public string AcctNum { get; set; }
        public string Person { get; set; }
        public string Zip { get; set; }
        public string Paycode { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string MName { get; set; }
        public string Suffix { get; set; }
        public string Gender { get; set; }
        public string Dob { get; set; }
        public string Ssn { get; set; }
        public string Ibex { get; set; }
        public int Doctor { get; set; }
        public string DoctorInit { get; set; }
        public int FirstDoctor { get; set; }
        public string FirstDoctorInit { get; set; }
        public int Resident { get; set; }
        public string ResidentInit { get; set; }
        public int Extender { get; set; }
        public string ExtenderInit { get; set; }
        public int DrExtender { get; set; }
        public string DrExtenderInit { get; set; }
        public int PrimaryNurse { get; set; }
        public string PrimaryNurseInit { get; set; }
        public int CareCoordinator { get; set; }
        public string CareCoordinatorInit { get; set; }
        public int Scribe { get; set; }
        public string ScribeInit { get; set; }
        public string UrgName { get; set; }
        public string Eun { get; set; }
        public string UrgColorCode { get; set; }
        public string UrgColorName { get; set; }
        public string UrgColorVal1 { get; set; }
        public string UrgColorVal2 { get; set; }
        public string AgeUnits { get; set; }
        public byte Age { get; set; }
        public string Readmit { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string EthnicityCode { get; set; }
        public string EthnicityName { get; set; }
        public string Complaint { get; set; }
        public string ComplaintCode { get; set; }
        public int LOSMins { get; set; }
        public string Dept { get; set; }
        public string Ward { get; set; }
        public string Ward2 { get; set; }
        public string Bed { get; set; }
        public string VitalIndicator { get; set; }
        public string VitalColorCode { get; set; }
        public string DispoCode { get; set; }
        public string DispoLoc { get; set; }
        public string DispoCodeName { get; set; }
        public string DispoLocName { get; set; }
        public string Ord0 { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public string Ord3 { get; set; }
        public string Ord4 { get; set; }
        public string Ord10 { get; set; }
        public string Ord11 { get; set; }
        public string Ord12 { get; set; }
        public string Ord13 { get; set; }
        public string Ord14 { get; set; }
        public string Ord15 { get; set; }
        public string Ord20 { get; set; }
        public string Ord21 { get; set; }
        public string Ord22 { get; set; }
        public string Ord23 { get; set; }
        public string Ord25 { get; set; }
        public string Ord26 { get; set; }
        public string Ord27 { get; set; }
        public string Ord29 { get; set; }
        public string Ord30 { get; set; }
        public string Ord46 { get; set; }
        public string Ord47 { get; set; }
        public string Ord56 { get; set; }
        public bool AllDone { get; set; }
        public Decimal? Height { get; set; }
        public Decimal Weight { get; set; }
        public string doctor_seen { get; set; }
    }

    public class RaceResultType
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class SignatureInfo
    {
        public List<DigitalSignature> DigitalSignatures { get; set; }
        public bool SignableEvents { get; set; }
    }

    public class SignupInfoResultType
    {
        public string DoctorSeenText { get; set; }
        public string ResidentSeenText { get; set; }
        public string DoctorExtenderSeenText { get; set; }
    }

    public class AuthorizationStatusResultType
    {
        public bool IsActive { get; set; }
    }

    public class VitalsResultType
    {
        public byte SiteId { get; set; }
        public string Ibex { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int User { get; set; }
        public string Date { get; set; }
    }

    public class BrandResultType
    {
        public string Brand { get; set; }
    }

    public class IbexContext : DbContext
    {
        public IbexContext() : base("IbexContext")
        {
            //Disable Initializer
            Database.SetInitializer<IbexContext>(null);
        }

        public IbexContext(SqlConnection conn, bool contextOwnsConnection = false) : base(existingConnection: conn, contextOwnsConnection: contextOwnsConnection)
        {

        }

        public const string schemaName = "api";

        public DbSet<Site> Sites { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<MobileDevice> Devices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserMapping> UserMapping { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Encounter> Encounters { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Medication.Component> MedicationComponents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SiteConfiguration());
            modelBuilder.Configurations.Add(new DepartmentConfiguration());
            modelBuilder.Configurations.Add(new BedConfiguration());
            modelBuilder.Configurations.Add(new UserConfiguration());
            modelBuilder.Configurations.Add(new AreaConfiguration());
            modelBuilder.Configurations.Add(new UserMappingConfiguration());
            modelBuilder.Configurations.Add(new MedicationConfiguration());
            modelBuilder.Configurations.Add(new MedicationComponentConfiguration());
            modelBuilder.Configurations.Add(new MobileDeviceConfiguration());

            modelBuilder.ComplexType<AllergyResultType>();
            modelBuilder.ComplexType<ClinicalPathwayResultType>();
            modelBuilder.ComplexType<CurrentMedicationResultType>();
            modelBuilder.ComplexType<CommentResultType>();
            modelBuilder.ComplexType<Demographics>();
            modelBuilder.ComplexType<ElementResultType>();
            modelBuilder.ComplexType<EncounterResultType>();
            modelBuilder.ComplexType<Ethnicity>();
            modelBuilder.ComplexType<FavoriteResultType>();
            modelBuilder.ComplexType<GroupResultType>();
            modelBuilder.ComplexType<LateResultResultType>();
            modelBuilder.ComplexType<OrderResultType>();
            modelBuilder.ComplexType<LocationResultType>();
            modelBuilder.ComplexType<MedMetaDataResultType>();
            modelBuilder.ComplexType<PatientResultType>();
            modelBuilder.ComplexType<RaceResultType>();
            modelBuilder.ComplexType<SignupInfoResultType>();
            modelBuilder.ComplexType<VitalsResultType>();
            modelBuilder.ComplexType<BrandResultType>();
            modelBuilder.ComplexType<ServiceResultType>();
            modelBuilder.ComplexType<AuthorizationStatusResultType>();

            modelBuilder.Conventions.Add(new FunctionsConvention<IbexContext>(schemaName));
        }

        [DbFunction("IbexContext", "GetCurrentPatients")]
        public IQueryable<PatientResultType> CurrentPatientsBySiteDept(byte siteId, string dept, int userId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);

            var deptParameter = dept != null
                ? new ObjectParameter("dept", dept)
                : new ObjectParameter("dept", typeof(string));

            var userIdParameter = new ObjectParameter("userId", userId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<PatientResultType>(
                    String.Format("[{0}].[GetCurrentPatients](@siteId, @dept, @userId)", GetType().Name), siteIdParameter, deptParameter, userIdParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientAllergies")]
        public IQueryable<AllergyResultType> GetPatientAllergies(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<AllergyResultType>(
                    String.Format("[{0}].[GetPatientAllergies](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientComments")]
        public IQueryable<CommentResultType> PatientComments(byte siteId, string patientId, bool trackingBoardOnly = false)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);
            var MTBOnlyParameter = new ObjectParameter("MTBOnly", trackingBoardOnly);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<CommentResultType>(
                    String.Format("[{0}].[GetPatientComments](@siteId, @ibex, @MTBOnly)", GetType().Name), siteIdParameter, patientIdParameter, MTBOnlyParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientCurrentMedications")]
        public IQueryable<CurrentMedicationResultType> GetPatientCurrentMedications(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<CurrentMedicationResultType>(
                    String.Format("[{0}].[GetPatientCurrentMedications](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientDetails")]
        public IQueryable<PatientResultType> PatientDetails(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<PatientResultType>(
                    String.Format("[{0}].[GetPatientDetails](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientEncounters")]
        public IQueryable<EncounterResultType> PatientEncounters(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<EncounterResultType>(
                    String.Format("[{0}].[GetPatientEncounters](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientLateResults")]
        public IQueryable<LateResultResultType> GetPatientLateResults(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<LateResultResultType>(
                    String.Format("[{0}].[GetPatientLateResults](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientOrders")]
        public IQueryable<OrderResultType> GetPatientOrders(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<OrderResultType>(
                    String.Format("[{0}].[GetPatientOrders](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetPatientRaces")]
        public IQueryable<RaceResultType> PatientRaces(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<RaceResultType>(
                    String.Format("[{0}].[GetPatientRaces](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetSiteGroups")]
        public IQueryable<GroupResultType> GetSiteGroups(byte siteId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<GroupResultType>(
                    String.Format("[{0}].[GetSiteGroups](@siteId)", GetType().Name), siteIdParameter
                );
        }

        [DbFunction("IbexContext", "GetSiteMedMetaData")]
        public IQueryable<MedMetaDataResultType> GetSiteMedMetaData(byte siteId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<MedMetaDataResultType>(
                    String.Format("[{0}].[GetSiteMedMetaData](@siteId)", GetType().Name), siteIdParameter
                );
        }

        [DbFunction("IbexContext", "GetSitePathways")]
        public IQueryable<GroupResultType> GetSitePathways(byte siteId, int pathwayNum = 0)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var pathwayIdParameter = new ObjectParameter("pathwayNum", pathwayNum);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<GroupResultType>(
                    String.Format("[{0}].[GetSitePathways](@siteId, @pathwayNum)", GetType().Name), siteIdParameter, pathwayIdParameter
                );
        }

        [DbFunction("IbexContext", "SearchSitePathways")]
        public IQueryable<ClinicalPathwayResultType> SearchClinicalPathways(byte siteId, string name, int limit)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var nameParameter = new ObjectParameter("name", name);
            var limitParameter = new ObjectParameter("limit", limit);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<ClinicalPathwayResultType>(
                    String.Format("[{0}].[SearchSitePathways](@siteId, @name, @limit)", GetType().Name), siteIdParameter, nameParameter, limitParameter
                );
        }

        [DbFunction("IbexContext", "SearchSiteMedications")]
        public IQueryable<BrandResultType> SearchSiteMedications(byte siteId, string brand, int limit)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var brandParameter = new ObjectParameter("brand", brand);
            var limitParameter = new ObjectParameter("limit", limit);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<BrandResultType>(
                    String.Format("[{0}].[SearchSiteMedications](@siteId, @brand, @limit)", GetType().Name), siteIdParameter, brandParameter, limitParameter
                );
        }        

        [DbFunction("IbexContext", "SearchSiteOrders")]
        public IQueryable<ServiceResultType> SearchSiteOrders(byte siteId, string name, int limit, int userId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var nameParameter = new ObjectParameter("name", name);
            var limitParameter = new ObjectParameter("limit", limit);
            var userParameter = new ObjectParameter("user", userId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<ServiceResultType>(
                    String.Format("[{0}].[SearchSiteOrders](@siteId, @name, @limit, @user)", GetType().Name), siteIdParameter, nameParameter, limitParameter, userParameter
                );
        }

        [DbFunction("IbexContext", "GetUserFavorites")]
        public IQueryable<FavoriteResultType> GetUserFavorites(byte siteId, int userId, string favType)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var drsnumParameter = new ObjectParameter("drsnum", userId);
            var favoriteType = new ObjectParameter("favoriteType", favType);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<FavoriteResultType>(
                    String.Format("[{0}].[GetUserFavorites](@siteId, @drsnum, @favoriteType)", GetType().Name), siteIdParameter, drsnumParameter, favoriteType
                );
        }

        [DbFunction("IbexContext", "GetUserFavoriteOrders")]
        public IQueryable<ServiceResultType> GetUserFavoriteOrders(int userId)
        {
            var userParameter = new ObjectParameter("user", userId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<ServiceResultType>(
                    String.Format("[{0}].[GetUserFavoriteOrders](@user)", GetType().Name), userParameter
                );
        }

        /// <summary>
        /// Get the list of Digital Signatures on a patient's chart, and whether the current user has signable events in the chart
        /// </summary>
        /// <remarks>Note this will only return the most recent signature for each user who has signed the chart</remarks>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>List of DigitalSignature objects</returns>
        public async Task<SignatureInfo> PatientDigitalSignaturesAsync(byte siteId, string patientId, User user)
        {
            var signatureList = new List<DigitalSignature>();
            var signatures = new Dictionary<int, DigitalSignature>();
            var emr = new EMR(siteId, patientId);
            var _t = new Time();
            var userUnsigned = false;
            foreach (EMR.Line line in emr.Lines)
            {
                if (line.NCT() == EMR.Constants.NCT_DIG_SIG)
                {
                    if (!line.IsInactive())
                    {
                        if (line.User() == user.Id)
                        {
                            userUnsigned = false;
                        }
                        var signature = new DigitalSignature
                        {
                            Date = Time.DateTimeFromString(line.SysTime())
                        };
                        var lineData = line.Data;
                        if (lineData.IndexOf("SIGNATURE PENDING") > 0)
                        {
                            signature.Status = Constants.PENDING_STATUS;
                        }
                        else
                        {
                            signature.Status = Constants.ACTIVE_STATUS;
                        }

                        signatures[line.User()] = signature;
                    }
                } else if (!line.SectionName().Equals(EMR.Constants.SECT_ADMIN) && !EMR.NoSignatureRequired(line.NCT(), line.PartName()) && line.User() == user.Id)
                {
                    // If the user removed a patient without entering any disposition codes, no signature is needed.
                    if (!(line.NCT() == EMR.Constants.NCT_DISPOSITION && line.DataSegments.Count == 1 && line.DataSegments[0].ChartWriter().Equals("s")))
                    {
                        var lastSign = signatures.ContainsKey(line.User()) ? signatures[line.User()] : null;
                        if (lastSign != null && lastSign.Date != null)
                        {
                            var lineDT = Time.DateTimeFromString(line.SysTime());
                            if (lineDT.HasValue && lastSign.Date < lineDT.Value)
                            {
                                userUnsigned = true;
                            }
                        }
                    }
                }
            }

            if (!userUnsigned)
            {
                var res = new DB.Select
                {
                    Sql = "SELECT usr FROM sigaud WHERE site=@site AND ibex=@ibex AND usr=@usr",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                        new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                        new SqlParameter("@usr", SqlDbType.Int) { Value = user.Id }
                    }
                }.RunForInt();
                if (res > 0)
                {
                    userUnsigned = true;
                }
            }
            
            if (signatures.Keys.Count > 0)
            {
                var sigUsers = signatures.Keys.ToList();
                var userRepo = new UserRepository(this);
                var users = await userRepo.GetUsersByIdAsync(sigUsers);
                foreach (int userId in sigUsers)
                {
                    DigitalSignature ds = signatures[userId];
                    var userMatch = users.First(x => x.Id == userId);
                    if (userMatch != null)
                    {
                        ds.User = new MinimalUser
                        {
                            Id = userId,
                            SiteId = userMatch.SiteId,
                            FirstName = userMatch.FirstName,
                            LastName = userMatch.LastName,
                            Initials = userMatch.Initials
                        };
                    }

                    signatureList.Add(ds);
                }

                signatureList.OrderBy(o => o.Status.Code).ThenByDescending(o => o.Date);
            }

            return new SignatureInfo
            {
                DigitalSignatures = signatureList,
                SignableEvents = userUnsigned
            };
        }

        /// <summary>
        /// Get the entire set of vital signs for a patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="p">Patient object</param>
        /// <param name="user">User object</param>
        /// <returns>List of VitalSign objects</returns>
        public List<VitalSign> FullPatientVitals(byte siteId, Patient p, User user)
        {
            var Vitals = new List<VitalSign>();
            string patientId = p.Ibex;
            var emr = new EMR(siteId, patientId);
            var previousLine = new EMR.Line();
            var enteredAssessments = new HashSet<string>();
            var formats = new List<string>();
            var customAssessments = 0;
            var chartDocIds = new Dictionary<string, EMR.Line>();
            var attributes = new Dictionary<string, List<EMR.Line.DataSegment>>();
            var userLookups = new HashSet<int>();
            var values = new List<EMR.Line>();

            // If no age, then default to 999 years.
            var ageInDays = AgeUtil.DaysOld(p.Demographics.Age.Value, p.Demographics.Age.Unit.ToString());
            if (ageInDays == 0)
            {
                ageInDays = AgeUtil.Constants.DAYS_IN_YEAR * 999;
            }
            var vsRanges = VitalSigns.GetVSRangeForAge(siteId, ageInDays);

            foreach (EMR.Line line in emr.Lines)
            {
                if (line.NCT() == EMR.Constants.NCT_FLOWSHEET_VS)
                {
                    var vsLabels = line.data();
                    var chartVsSections = new List<string>() { line.PartName() };
                    foreach (EMR.Line.DataSegment d in vsLabels)
                    {
                        chartVsSections.Add(d.ToString());
                    }
                    if (!line.PartName().Equals(EMR.Constants.SECT_VITAL_SIGNS))
                    {
                        if (formats.Count() == 0)
                        {
                            formats.AddRange(VitalSigns.GetVitalSignSections(siteId));
                        }
                        customAssessments++;
                    }
                    formats.AddRange(chartVsSections);

                }
                else if (line.NCT() == EMR.Constants.NCT_FLOWSHEET)
                {
                    if (previousLine.PartName().Equals(EMR.Constants.SECT_VITAL_SIGNS))
                    {
                        // Catch obsolete entry methods with a mismatch of the doc_id
                        chartDocIds.Add(line.DocId(), previousLine);
                    }
                    var vsData = line.data();
                    userLookups.Add(line.User());
                    if (line.SectionName().Equals(EMR.Constants.SECT_FLOWSHEET_ATTRIB))
                    {
                        // TODO: MOB-175
                        // There's an error in core PulseCheck that inserts two lines of data for 
                        // flowsheet attributes.  One is the legit data, and the other is just a bunch 
                        // of ampersands.  That should be fixed, and then this hack can be removed.
                        var tempVsData = Regex.Replace(line.Data, "&", "");
                        if (!string.IsNullOrWhiteSpace(tempVsData))
                            attributes.Add(line.ChartXRef(), vsData);
                    }
                    else
                    {
                        values.Add(line);
                    }
                }
                else if (!String.IsNullOrEmpty(line.DocId()) && line.PartName().Equals(EMR.Constants.SECT_VITAL_SIGNS))
                {
                    chartDocIds.Add(line.DocId(), line);
                }
                else
                {
                    previousLine = line;
                }
            }

            values = values.OrderBy(o => o.UserTime()).ThenBy(o => o.SysTime()).ToList();

            if (formats.Count < 1)
            {
                formats.AddRange(VitalSigns.GetVitalSignSections(siteId));
            }

            for (int count = 0; count < values.Count(); count++)
            {
                var reverseIndex = values.Count() - 1 - count;
                var vitalLine = values[reverseIndex];
                var vitalSigns = vitalLine.data();
                var vitalSignEntryCount = vitalSigns.Count();
                var vitalTypes = VitalSigns.GetVitalSignType();

                // These entries are before we added MAP and any new vital sign types. Need to do some padding.
                while (vitalSigns.Count < VitalSigns.Constants.EXPECTED_CORE_VITALS_COUNT)
                {
                    vitalSigns.Add(new EMR.Line.DataSegment(""));
                }

                for (int index = 0; index < vitalSignEntryCount; index++)
                {
                    var type = vitalTypes[index];
                    var value = VitalSigns.RemoveDashes(vitalSigns[index].ToString());

                    var userTime = vitalLine.UserTime();
                    var entryTime = String.IsNullOrEmpty(userTime) ? vitalLine.SysTime() : userTime;
                    if (index == 6 && !String.IsNullOrEmpty(entryTime))
                    {
                        value = VitalSigns.DateFormat(entryTime, siteId);
                    }

                    var attribute = "";
                    var chartXref = vitalLine.ChartXRef();
                    if (!String.IsNullOrEmpty(chartXref) && attributes.ContainsKey(chartXref) && !String.IsNullOrEmpty(attributes[chartXref][index].ToString()))
                    {
                        attribute = attributes[chartXref][index].ToString();

                        // O2 sat
                        if (index == 5)
                        {
                            attribute = Regex.Replace(attribute, @"^\s*on:?\s+", "", RegexOptions.IgnoreCase).Trim();
                        }

                        if (!String.IsNullOrEmpty(attribute) && !attribute.StartsWith("("))
                        {
                            // Multi attribute entry
                            if (attribute.Substring(0, 2).Equals(", "))
                            {
                                // No entry for first attribute
                                attribute = attribute.Substring(2).Trim();
                            }
                            attribute = "(" + attribute + ")";
                        }
                    }

                    if (type.Equals(VitalSigns.Constants.BP))
                    {
                        var pieces = value.Split('/');
                        if (!String.IsNullOrWhiteSpace(pieces[0]))
                        {
                            VitalSign vsBPSys = new VitalSign
                            {
                                Type = type + " Systolic",
                                Value = pieces[0],
                                Attribute = attribute,
                                DateTime = Time.DateTimeFromString(entryTime),
                                Status = vitalLine.IsInactive() ? Constants.INACTIVE_STATUS : Constants.ACTIVE_STATUS,
                                User = new MinimalUser
                                {
                                    Id = vitalLine.User(),
                                    Initials = emr.UserInitials(vitalLine.User()),
                                    SiteId = siteId
                                }
                            };
                            Vitals.Add(vsBPSys);
                        }

                        if (pieces.Length > 1 && !String.IsNullOrWhiteSpace(pieces[1]))
                        {
                            VitalSign vsBPDia = new VitalSign
                            {
                                Type = type + " Diastolic",
                                Value = pieces[1],
                                Attribute = attribute,
                                DateTime = Time.DateTimeFromString(entryTime),
                                Status = vitalLine.IsInactive() ? Constants.INACTIVE_STATUS : Constants.ACTIVE_STATUS,
                                User = new MinimalUser
                                {
                                    Id = vitalLine.User(),
                                    Initials = emr.UserInitials(vitalLine.User()),
                                    SiteId = siteId
                                }
                            };

                            Vitals.Add(vsBPDia);
                        }
                    }
                    else
                    {
                        VitalSign vs = new VitalSign
                        {
                            Type = type,
                            Value = value,
                            Attribute = attribute,
                            DateTime = Time.DateTimeFromString(entryTime),
                            Status = vitalLine.IsInactive() ? Constants.INACTIVE_STATUS : Constants.ACTIVE_STATUS,
                            User = new MinimalUser
                            {
                                Id = vitalLine.User(),
                                Initials = emr.UserInitials(vitalLine.User()),
                                SiteId = siteId
                            }
                        };

                        Vitals.Add(vs);
                    }
                }
            }

            // Loop over resulting vitals and apply style information based on vital ranges
            foreach(VitalSign vs in Vitals)
            {
                vs.Status.Style = null;
                var type = vs.Type;
                if (String.IsNullOrEmpty(vs.Value) || vsRanges == null || !vsRanges.ContainsKey(type))
                {
                    continue;
                }

                var strValue = Regex.Replace(vs.Value, @"[^\d\.\-]", "");
                double value;
                Double.TryParse(strValue, out value);
                var rangeInfo = vsRanges[type];
                var panicLow = rangeInfo[VitalSigns.Constants.RANGE_PANIC_LOW];
                var normalLow = rangeInfo[VitalSigns.Constants.RANGE_NORMAL_LOW];
                var normalHigh = rangeInfo[VitalSigns.Constants.RANGE_NORMAL_HIGH];
                var panicHigh = rangeInfo[VitalSigns.Constants.RANGE_PANIC_HIGH];

                if (panicLow != null && value <= panicLow)
                {
                    vs.Style = new Style(VitalSigns.GetVitalStyleInfo(VitalSigns.Constants.PANIC_LOW_CODE));
                } else if (normalLow != null && value < normalLow)
                {
                    vs.Style = new Style(VitalSigns.GetVitalStyleInfo(VitalSigns.Constants.WARN_LOW_CODE));
                } else if (panicHigh != null && value >= panicHigh)
                {
                    vs.Style = new Style(VitalSigns.GetVitalStyleInfo(VitalSigns.Constants.PANIC_HIGH_CODE));
                } else if (normalHigh != null && value > normalHigh)
                {
                    vs.Style = new Style(VitalSigns.GetVitalStyleInfo(VitalSigns.Constants.WARN_HIGH_CODE));
                } else
                {
                    vs.Style = new Style(VitalSigns.GetVitalStyleInfo(VitalSigns.Constants.NORMAL_CODE));
                }
            }

            return Vitals;
        }

        /// <summary>
        /// Get the current vitals for the patient (only what appears on the MTB)
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns></returns>
        [DbFunction("IbexContext", "GetPatientVitals")]
        public IQueryable<VitalsResultType> LatestPatientVitals(byte siteId, string patientId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<VitalsResultType>(
                    String.Format("[{0}].[GetPatientVitals](@siteId, @ibex)", GetType().Name), siteIdParameter, patientIdParameter
                );
        }

        [DbFunction("IbexContext", "GetSiteComments")]
        public IQueryable<ElementResultType> CommentsBySiteId(byte siteId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<ElementResultType>(
                    String.Format("[{0}].[GetSiteComments](@siteId)", GetType().Name), siteIdParameter
                );
        }

        [DbFunction("IbexContext", "GetSiteLocations")]
        public IQueryable<LocationResultType> AvailableLocationsBySiteId(byte siteId, string dept = null)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var deptParameter = dept != null
                ? new ObjectParameter("dept", dept)
                : new ObjectParameter("dept", typeof(string));

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<LocationResultType>(
                    String.Format("[{0}].[GetSiteLocations](@siteId, @dept)", GetType().Name), siteIdParameter, deptParameter
                );
        }

        [DbFunction("IbexContext", "GetSiteShareLocations")]
        public IQueryable<LocationResultType> ShareLocationsBySiteId(byte siteId, string dept = null)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var deptParameter = dept != null
                ? new ObjectParameter("dept", dept)
                : new ObjectParameter("dept", typeof(string));

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<LocationResultType>(
                    String.Format("[{0}].[GetSiteShareLocations](@siteId, @dept)", GetType().Name), siteIdParameter, deptParameter
                );
        }

        [DbFunction("IbexContext", "SetPatientLocation")]
        public IQueryable<LocationResultType> SetPatientLocation(string location, string patientId, byte siteId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);
            var patientIdParameter = new ObjectParameter("ibex", patientId);
            var locationParameter = new ObjectParameter("location", location);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<LocationResultType>(
                    String.Format("[{0}].[SetPatientLocation] @siteId, @ibex, @location", GetType().Name), siteIdParameter, patientIdParameter, locationParameter
                );
        }

        [DbFunction("IbexContext", "GetSiteSignupInfo")]
        public IQueryable<SignupInfoResultType> GetSignupInfo(byte siteId)
        {
            var siteIdParameter = new ObjectParameter("siteId", siteId);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<SignupInfoResultType>(
                    String.Format("[{0}].[GetSiteSignupInfo](@siteId)", GetType().Name), siteIdParameter
                );
        }

        [DbFunction("IbexContext", "AddUserFavoriteOrder")]
        public ObjectResult<int> AddUserFavoriteOrder(int userId, int favoriteNum)
        {
            var typeParameter = new SqlParameter("type", "O");
            var userIdParameter = new SqlParameter("drsNum", userId);
            var favoriteNumParameter = new SqlParameter("favoriteNum", favoriteNum);

            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[AddUserFavorite] @drsNum, @type, @favoriteNum", schemaName), userIdParameter, typeParameter, favoriteNumParameter
                );
        }

        [DbFunction("IbexContext", "CreateAuthorizationCode")]
        public ObjectResult<string> CreateAuthorizationCode()
        {
            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<string>(
                    String.Format("EXEC [{0}].[CreateAuthorizationCode]", schemaName)
                );
        }

        [DbFunction("IbexContext", "CheckAuthorizationCode")]
        public IQueryable<AuthorizationStatusResultType> CheckAuthorizationCode(string authorizationCode)
        {
            var codeParameter = new ObjectParameter("authorization_code", authorizationCode);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<AuthorizationStatusResultType>(
                    String.Format("[{0}].[CheckAuthorizationCode](@authorization_code)", GetType().Name), codeParameter
                );
        }

        [DbFunction("IbexContext", "DeleteDevice")]
        //public async Task<ObjectResult<int>> DeleteDevice(string deviceId)
        public ObjectResult<int> DeleteDevice(string deviceId)
        {
            var deviceParameter = new SqlParameter("device_id", deviceId);

            var res = ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[DeleteDevice] @device_id", schemaName), deviceParameter
                );
            return res;
        }

        [DbFunction("IbexContext", "AddDevice")]
        public ObjectResult<int> AddDevice(string deviceId, string os, string osVersion, string manufacturer, string model, string friendlyName)
        {
            var idParameter = new SqlParameter("device_id", deviceId);
            var osParameter = new SqlParameter("os", os);
            var osVersionParameter = new SqlParameter("os_version", osVersion);
            var manufacturerParameter = new SqlParameter("manufacturer", manufacturer);
            var modelParameter = new SqlParameter("model", model);
            var friendlyNameParameter = new SqlParameter("friendly_name", friendlyName);

            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[AddDevice] @device_id, @os, @os_version, @manufacturer, @model, @friendly_name", schemaName), 
                    idParameter, osParameter, osVersionParameter, manufacturerParameter, modelParameter, friendlyNameParameter
                );
        }

        [DbFunction("IbexContext", "RemoveAccountUser")]
        public ObjectResult<int> RemoveAccountUser(string login, int userId)
        {
            var loginParameter = new SqlParameter("login", login);
            var userNumParameter = new SqlParameter("usernum", userId);

            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[RemoveAccountUser] @login, @usernum", schemaName), loginParameter, userNumParameter
                );
        }

        [DbFunction("IbexContext", "RemoveAllAccountUsers")]
        public ObjectResult<int> RemoveAllAccountUsers(string login)
        {
            var loginParameter = new SqlParameter("login", login);

            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[RemoveAllAccountUsers] @login", schemaName), loginParameter
                );
        }

        [DbFunction("IbexContext", "AddAccountUser")]
        public ObjectResult<int> AddAccountUser(string login, int userId)
        {
            var loginParameter = new SqlParameter("login", login);
            var userNumParameter = new SqlParameter("usernum", userId);

            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[AddAccountUser] @login, @usernum", schemaName), loginParameter, userNumParameter
                );
        }

        [DbFunction("IbexContext", "RemoveUserFavoriteOrder")]
        public ObjectResult<int> RemoveUserFavoriteOrder(int userId, int favoriteNum)
        {
            var typeParameter = new SqlParameter("type", "O");
            var userIdParameter = new SqlParameter("drsNum", userId);
            var favoriteNumParameter = new SqlParameter("favoriteNum", favoriteNum);

            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[RemoveUserFavorite] @drsNum, @type, @favoriteNum", schemaName), userIdParameter, typeParameter, favoriteNumParameter
                );
        }

        [DbFunction("IbexContext", "SetPatientComment")]
        public ObjectResult<int> PostPatientCommentById(byte siteId, string patientId, int userId, Int32? commentId, string commentName = null, bool removeComment = false)
        {
            var siteIdParameter = new SqlParameter("siteId", siteId);
            var patientIdParameter = new SqlParameter("ibex", patientId);
            var userIdParameter = new SqlParameter("userId", userId);

            var commentIdParameter = new SqlParameter("commentId", SqlDbType.Int);
            commentIdParameter.Value = DBNull.Value;
            if (commentId != null)
            {
                commentIdParameter.Value = commentId;
            }

            // Structured comment ID provided - look up comment info on the DB side.
            if (commentId > 0)
            {
                commentName = null;
            }

            // New structured comment provided, but we're trying to remove an MTB comment. Can't do that.
            if (commentId > 3 && removeComment)
            {
                removeComment = false;
            }

            var commentNameParameter = new SqlParameter("commentName", SqlDbType.Char, 80);
            commentNameParameter.Value = DBNull.Value;
            if (commentName != null)
            {
                commentNameParameter.Value = commentName;
            }

            var removeCommentParameter = new SqlParameter("removeComment", SqlDbType.Bit);
            removeCommentParameter.Value = removeComment;

            return ((IObjectContextAdapter)this).ObjectContext
                .ExecuteStoreQuery<int>(
                    String.Format("EXEC [{0}].[SetPatientComment] @siteId, @ibex, @userId, @commentId, @commentName, @removeComment", schemaName), siteIdParameter, patientIdParameter, userIdParameter, commentIdParameter, commentNameParameter, removeCommentParameter
                );
        }
    }

    public class SiteConfiguration : EntityTypeConfiguration<Site>
    {
        public SiteConfiguration()
        {
            //Primary Key
            HasKey(t => t.Id);
            
            //Properties
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(t => t.Name)
                .HasMaxLength(40)
                .IsRequired();

            //Table & Column Mappings
            ToTable("org");
            Property(t => t.Id).HasColumnName("site");
            Property(t => t.Name).HasColumnName("name");
            Property(t => t.Timeout).HasColumnName("timeout");
            Property(t => t.Root).HasColumnName("root");
            Property(t => t.Status.Code)
                .IsRequired()
                .HasColumnName("status");
        }
    }

    public class BedConfiguration : EntityTypeConfiguration<Bed>
    {
        public BedConfiguration()
        {
            ToTable("bed");
            HasKey(t => new
            {
                t.Name,
                t.Ward,
                t.Dept,
                t.SiteId
            });
            Property(t => t.SiteId)
                .IsRequired()
                .HasColumnName("site");
            Property(t => t.Name).HasColumnName("bed");
            Property(t => t.Dept).HasColumnName("dept");
            Property(t => t.Ward).HasColumnName("ward");
        }
    }

    public class DepartmentConfiguration : EntityTypeConfiguration<Department>
    {
        public DepartmentConfiguration()
        {
            //Primary Key
            HasKey(t => new
            {
                t.Dept,
                t.SiteId
            });
            
            //Properties
            Property(t => t.Dept)
                .IsRequired()
                .HasMaxLength(4);
 
            Property(t => t.Name)
                .HasMaxLength(40)
                .IsRequired();

            //Table & Column Mappings
            ToTable("dept");
            Property(t => t.Dept).HasColumnName("dept");
            Property(t => t.SiteId).HasColumnName("site");
            Property(t => t.Name).HasColumnName("name");
            Property(t => t.Status.Code)
                .IsRequired()
                .HasColumnName("status");
        }
    }
    public class UserConfiguration : EntityTypeConfiguration<User>
    {
        public UserConfiguration()
        {
            //Primary Key
            HasKey(t => t.Id);

            //Properties
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //Table & Column Mappings
            ToTable("basic_user_vw", "api");
            Property(t => t.Id).HasColumnName("num");
            Property(t => t.SiteId).HasColumnName("site");
            Property(t => t.SiteName).HasColumnName("site_name");
            Property(t => t.LastName).HasColumnName("last");
            Property(t => t.FirstName).HasColumnName("first");
            Property(t => t.Type).HasColumnName("type");
            Property(t => t.TrackFilters).HasColumnName("trackfilters");
            Property(t => t.DeptView).HasColumnName("deptview");
            Property(t => t.OrderingOnly).HasColumnName("ordonly");
            Property(t => t.Subordinate).HasColumnName("subord");
            Property(t => t.Status.Code)
                .IsRequired()
                .HasColumnName("status");

            //Ignore
            Ignore(t => t.MiddleName);
            Ignore(t => t.Suffix);
            Ignore(t => t.Prefix);
        }
    }

    public class MedicationConfiguration : EntityTypeConfiguration<Medication>
    {
        public MedicationConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasKey(t => new { t.Ibex, t.Site, t.Losecs });

            // Table and column mappings
            ToTable("med");
            Property(t => t.Ibex).HasColumnType("char");
            Property(t => t.Site);
            Property(t => t.Losecs);
            Property(t => t.Status);
            Property(t => t.Type);
            Property(t => t.Name);
            Property(t => t.Route);
            Property(t => t.Unit);
            Property(t => t.Schedule);
            Property(t => t.Dose);
            Property(t => t.Time).HasColumnName("med_time");
            Property(t => t.Repeat).HasColumnName("med_repeat");
            Property(t => t.Notes).HasColumnName("med_notes");
            Property(t => t.Barcode).HasColumnName("med_barcode");
            Property(t => t.OrderDate).HasColumnName("order_date");
            Property(t => t.AckDate).HasColumnName("ack_date");
            Property(t => t.HoldDate).HasColumnName("hold_date");
            Property(t => t.HoldSysdate).HasColumnName("hold_sysdate");
            Property(t => t.UnholdDate).HasColumnName("unhold_date");
            Property(t => t.UnholdSysdate).HasColumnName("unhold_sysdate");
            Property(t => t.CancelDate).HasColumnName("cancel_date");
            Property(t => t.CancelSysdate).HasColumnName("cancel_sysdate");
            Property(t => t.DeleteDate).HasColumnName("delete_date");
            Property(t => t.GiveDate).HasColumnName("give_date");
            Property(t => t.GiveSysdate).HasColumnName("give_sysdate");
            Property(t => t.OrderForUserId).HasColumnName("order_for_usr");
            Property(t => t.OrderUserId).HasColumnName("order_usr");
            Property(t => t.AckUserId).HasColumnName("ack_usr");
            Property(t => t.HoldUserId).HasColumnName("hold_usr");
            Property(t => t.UnholdUserId).HasColumnName("unhold_usr");
            Property(t => t.CancelUserId).HasColumnName("cancel_usr");
            Property(t => t.DeleteUserId).HasColumnName("delete_usr");
            Property(t => t.GiveUserId).HasColumnName("give_usr");
            Property(t => t.IVType).HasColumnName("iv_type");
            Property(t => t.StopUserId).HasColumnName("stop_usr");
            Property(t => t.StopDate).HasColumnName("stop_date");
            Property(t => t.IVSite).HasColumnName("iv_site");
            Property(t => t.IVLocation).HasColumnName("iv_location");
            Property(t => t.ExcludeUserId).HasColumnName("exclude_usr");
            Property(t => t.CPTLosecsLink).HasColumnName("cpt_losecslink");
            Property(t => t.StopSysdate).HasColumnName("stop_sysdate");
            Property(t => t.Authentication);
            Property(t => t.DiscontinueUserId).HasColumnName("discontinue_usr");
            Property(t => t.DiscontinuedUserId).HasColumnName("discontinued_usr");
            Property(t => t.DiscontinueDate).HasColumnName("discontinue_date");
            Property(t => t.DiscontinuedDate).HasColumnName("discontinued_date");
            Property(t => t.DiscontinueSysdate).HasColumnName("discontinue_sysdate");
            Property(t => t.DiscontinuedSysdate).HasColumnName("discontinued_sysdate");
            Property(t => t.Rate);
            Property(t => t.RateUnit).HasColumnName("rate_unit");
            Property(t => t.Indication);
        }
    }

    public class MedicationComponentConfiguration : EntityTypeConfiguration<Medication.Component>
    {
        public MedicationComponentConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Table and column mappings
            ToTable("med_details");
            Property(t => t.Ibex).HasColumnType("char");
            Property(t => t.Site);
            Property(t => t.Losecs);
            Property(t => t.Type);
            Property(t => t.BrandName).HasColumnName("brand_name");
            Property(t => t.ActiveName).HasColumnName("active_name");
            Property(t => t.DrugRoute).HasColumnName("drug_route");
            Property(t => t.DrugForm).HasColumnName("drug_form");
            Property(t => t.DrugStrength).HasColumnName("drug_strength");
            Property(t => t.EnteredDose).HasColumnName("entered_dose");
            Property(t => t.EnteredUnit).HasColumnName("entered_unit");
            Property(t => t.DrugDBType).HasColumnName("drug_db_type");
            Property(t => t.ActiveId).HasColumnName("active_id");
            Property(t => t.DrugId).HasColumnName("drug_id");
            Property(t => t.PackagingId).HasColumnName("packaging_id");
            Property(t => t.DrugCategoryId).HasColumnName("drug_category_id");            
            Property(t => t.DrugFormId).HasColumnName("drug_form_id");

            // Relationship  
            HasRequired(t => t.Medication).WithMany(c => c.Components).HasForeignKey
                    (t => new { t.Ibex, t.Site, t.Losecs }).WillCascadeOnDelete(false);
        }
    }

    public class MobileDeviceConfiguration : EntityTypeConfiguration<MobileDevice>
    {
        public MobileDeviceConfiguration()
        {
            HasKey(t => t.MobileDeviceId);
            Property(t => t.MobileDeviceId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //Table & Column Mappings
            ToTable("mobile_device", "dbo");
            Property(t => t.MobileDeviceId).HasColumnName("mobile_device_id");
            Property(t => t.DeviceId).HasColumnName("device_id");
            Property(t => t.OS).HasColumnName("os");
            Property(t => t.OSVersion).HasColumnName("os_version");
            Property(t => t.Manufacturer).HasColumnName("manufacturer");
            Property(t => t.Model).HasColumnName("model");
            Property(t => t.FriendlyName).HasColumnName("friendly_name");
            Property(t => t.IsAuthorized).HasColumnName("authorized");
        }
    }

    public class UserMappingConfiguration : EntityTypeConfiguration<UserMapping>
    {
        public UserMappingConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //Table & Column Mappings
            ToTable("master_login_vw", "api");
            Property(t => t.Id).HasColumnName("id");
            Property(t => t.Login).HasColumnName("login");
            Property(t => t.DomainLogin).HasColumnName("domainlogin");
            Property(t => t.UserNum).HasColumnName("usernum");
            Property(t => t.SiteId).HasColumnName("siteid");
            Property(t => t.Ctr).HasColumnName("ctr");
            Property(t => t.Retry).HasColumnName("retry");
            Property(t => t.WindowsDomains).HasColumnName("windowsdomains");
            Property(t => t.SiteName).HasColumnName("site_name");
        }
    }
    
    public class AreaConfiguration : EntityTypeConfiguration<Area>
    {
        public AreaConfiguration()
        {
            //Table & Column Mappings
            ToTable("ward");
            Property(t => t.Dept)
                .IsRequired()
                .HasColumnName("dept");
            
            Property(t => t.SiteId)
                .IsRequired()
                .HasColumnName("site");
            Property(t => t.Name).HasColumnName("name");
            Property(t => t.Status.Code)
                .IsRequired()
                .HasColumnName("status");
            Property(t => t.Type)
                .IsRequired()
                .HasColumnName("type");

            //Property(t => t.Id).HasColumnName("id");
        }
    }
}
