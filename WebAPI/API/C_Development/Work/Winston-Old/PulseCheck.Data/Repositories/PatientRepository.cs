using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.Domain.Options;
using PulseCheck.IData;
using PulseCheck.IRepository;
using PulseCheck.Utilities;
using Chart = PulseCheck.Domain.Chart;

namespace PulseCheck.Data.Repositories
{
    public class PatientRepository : BaseRepository, IPatientRepository
    {
        public PatientRepository(IbexContext context) : base(context)
        {

        }

        /// <summary>
        /// Get a patient by site and ibex number. Optionally expand certain data that we wouldn't want to retrieve every time.
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="expand">Optional expand parameter. Comma-delimited expand items</param>
        /// <returns>Patient object</returns>
        public async Task<Patient> GetPatientByIdAsync(byte siteId, string patientId, User user, string expand = "")
        {
            var result = await _context.PatientDetails(siteId, patientId).ToListAsync();
            if (result != null && result.Count > 0)
            {
                var patient = GetPatient(result[0], user);
                patient = await ExpandPatient(patient, siteId, user, expand);
                return patient;
            }

            return null;
        }

        /// <summary>
        /// Get a patient's allergies list
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns></returns>
        public async Task<List<Allergy>> GetPatientAllergies(byte siteId, string patientId, User user)
        {
            var result = await _context.GetPatientAllergies(siteId, patientId).ToListAsync();
            return result.OrderBy(o => o.ActionStatus).ThenBy(o => o.DateChg).ThenBy(o => o.Name).Select(c => new Allergy
            {
                Name = c.Name,
                Reaction = new Identifier
                {
                    Name = c.ReactionName,
                    Value = c.ReactionCode
                },
                Severity = new Identifier
                {
                    Name = c.SeverityName,
                    Value = c.SeverityCode
                },
                Source = new Identifier
                {
                    Name = c.SourceName,
                    Value = c.SourceCode
                },
                Comment = c.Comment,
                Status = Status.GetStatusByCode(c.Status),
                ActionStatus = Status.GetStatusByCode(c.ActionStatus),
                User = c.User > 0 ? new MinimalUser
                {
                    Id = c.User,
                    Initials = c.UserInit
                } : null,
                UserChg = c.UserChg > 0 ? new MinimalUser
                {
                    Id = c.UserChg,
                    Initials = c.UserChgInit
                } : null,
                DateAdd = Time.DateTimeFromString(c.DateAdd),
                DateChg = Time.DateTimeFromString(c.DateChg)
            }).ToList();
        }

        /// <summary>
        /// Get a patient's current medications list
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns></returns>
        public async Task<List<CurrentMedication>> GetPatientCurrentMedications(byte siteId, string patientId, User user)
        {
            var result = await _context.GetPatientCurrentMedications(siteId, patientId).ToListAsync();
            return result.OrderBy(o => o.ActionStatus).ThenBy(o => o.DateChg).ThenBy(o => o.Name).Select(c => new CurrentMedication
            {
                Name = c.Name,
                Dose = c.Dose,
                Unit = new Identifier
                {
                    Name = c.UnitName,
                    Value = c.UnitCode
                },
                Route = new Identifier
                {
                    Name = c.RouteName,
                    Value = c.RouteCode
                },
                Schedule = new Identifier
                {
                    Name = c.ScheduleName,
                    Value = c.ScheduleCode
                },
                LastTaken = c.LastTaken,
                Comment = c.Comment,
                Status = Status.GetStatusByCode(c.Status),
                ActionStatus = Status.GetStatusByCode(c.ActionStatus),
                User = c.User > 0 ? new MinimalUser
                {
                    Id = c.User,
                    Initials = c.UserInit
                } : null,
                UserChg = c.UserChg > 0 ? new MinimalUser
                {
                    Id = c.UserChg,
                    Initials = c.UserChgInit
                } : null,
                DateAdd = Time.DateTimeFromString(c.DateAdd),
                DateChg = Time.DateTimeFromString(c.DateChg)
            }).ToList();
        }

        /// <summary>
        /// Get a list of Orders with late results for a particular patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>List of Order objects</returns>
        public async Task<List<Order>> GetPatientLateResults(byte siteId, string patientId, User user)
        {
            var result = await _context.GetPatientLateResults(siteId, patientId).ToListAsync();
            return result.OrderByDescending(o => o.DteOrder).ThenBy(o => o.OrdName).Select(l => new Order
            {
                Id = l.OrderNumber,
                Losecs = l.Losecs,
                Name = l.OrdName,
                ServiceCode = l.AlienKey,
                Events = new List<Event>
                {
                    new Event
                    {
                        Type = "ORDERED",
                        Description = "Ordered",
                        DateTime = Time.DateTimeFromString(l.DteOrder).Value,
                        User = new MinimalUser
                        {
                            Id = l.UsrOrder,
                            Initials = l.UsrOrderInit,
                            SiteId = siteId
                        }
                    }
                }                
            }).ToList();
        }

        public async Task<List<Order>> GetPatientOrders(byte siteId, string patientId)
        {
            var result = await _context.GetPatientOrders(siteId, patientId).ToListAsync();

            return result.Select(x => new Order
            {
                Id = x.Id,
                Losecs = x.Losecs,
                Name = x.Name,
                ServiceCode = x.ServiceCode,
                Type = x.Type,
                StatusCode = x.StatusCode,
                OrderingPhysician = x.OrderingPhysician,
                Events = new List<Event>
                {
                    new Event
                    {
                        Type = "ORDERED",
                        Description = "Ordered",
                        DateTime = Time.DateTimeFromString(x.OrderDate).Value.ToUniversalTime(),
                        User = new MinimalUser
                        {
                            Id = x.Orderer,
                            Initials = x.OrdererInit,
                            SiteId = siteId
                        }
                    }
                }
            }).ToList();
        }

        /// <summary>
        /// Get the comments for a patient. Optionally only get the comments that would appear on the tracking board.
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="trackingBoardOnly">Flag for whether the return should only include tracking board comments</param>
        /// <returns>List of Comment objects</returns>
        public async Task<List<Comment>> GetPatientComments(byte siteId, string patientId, bool trackingBoardOnly = false)
        {
            var result = await _context.PatientComments(siteId, patientId, trackingBoardOnly).ToListAsync();
            return result.OrderByDescending(o => o.Date).ThenBy(o => o.Losecs).Select(c => new Comment()
            {
                Text = c.Comment,
                CommentNumOnTrackingBoard = c.CommentNum,
                DateTime = Time.DateTimeFromString(c.Date),
                User = new MinimalUser()
                {
                    Id = c.User,
                    Initials = c.UserInit
                },
                Style = new Style()
                {
                    ColorCode = c.ColorCode,
                    ColorName = c.ColorName,
                    ColorValues = { c.ColorVal1, c.ColorVal2 }
                }
            }).ToList();
        }

        /// <summary>
        /// Get the encounters for a patient.
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>List of Encounter objects</returns>
        public async Task<List<Encounter>> GetPatientEncounters(byte siteId, string patientId, User user)
        {
            var result = await _context.PatientEncounters(siteId, patientId).ToListAsync();
            var includeComplaints = user.HasAtLeastReadPermission(Permission.VIEW_COMPLAINT);
            return result.Select(e => new Encounter()
            {
                Ibex = e.Ibex,
                Date = Time.DateTimeFromString(e.Ibex),
                Site = new MinimalSite()
                {
                    Id = e.SiteId,
                    Name = e.SiteName
                },
                Complaint = includeComplaints ? new Complaint()
                {
                    Name = e.Complaint,
                    Code = e.ComplaintCode
                } : null,
                Diagnosis = e.Diagnosis,
                DispoCode = GetDispoCode(e.DispoCode, e.DispoCodeName),
                DispoLocation = GetDispoLocation(e.DispoLoc, e.DispoLocName),
                Providers = GetProviders(e)
            }).ToList();
        }

        /// <summary>
        /// Get patients in a site and department, optimized for displaying a tracking board.
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Department identifier</param>
        /// <param name="user">User object</param>
        /// <param name="filter">Optional filter type</param>
        /// <returns>List of Patient objects</returns>
        public async Task<List<Patient>> GetPatientsBySiteAndDeptForMTBAsync(byte siteId, string dept, User user, string filter = "")
        {
            var site = new Site(siteId);
            var blueCode = site.GetOrgOption("ORDERS_BLUE_CODE");
            var orderClassResults = new DB.Select { Sql = "SELECT * FROM lu_codes WHERE [type] = 'ORD_CLASS'" }.RunForListOfDictionaries();
            var orderClassInfo = new Dictionary<string, Dictionary<string, string>>();
            foreach(var info in orderClassResults)
            {
                orderClassInfo[info["pc_id"]] = new Dictionary<string, string>
                {
                    { "Name", info["name"] },
                    { "Value1", info["value1"] },
                    { "Value2", !string.IsNullOrWhiteSpace(info["value2"]) ? info["value2"] : null }
                };
            }

            var result = await _context.CurrentPatientsBySiteDept(siteId, dept, user.Id).ToListAsync();
            var patients = result.Select(p => GetPatientForMTB(p, user, blueCode, orderClassInfo)).ToList();

            return patients;
        }

        /// <summary>
        /// Get patients in a site and department.
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Department identifier</param>
        /// <param name="user">User object</param>
        /// <param name="expand">Optional expand parameter. Comma-delimited expand items</param>
        /// <returns>List of Patient objects</returns>
        public async Task<List<Patient>> GetPatientsBySiteAndDeptAsync(byte siteId, string dept, User user, string expand = "")
        {
            // Passing user ID 0 here so that the back end knows not to perform any filtering on the results.
            var result = await _context.CurrentPatientsBySiteDept(siteId, dept, 0).ToListAsync();
            var patients = new List<Patient>();
            foreach (var p in result) {
                patients.Add(GetPatient(p, user));
            }
            patients = await ExpandPatients(patients, siteId, user, expand);
            return patients;
        }

        /// <summary>
        /// Post a comment on a patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="commentId">Optional comment identifier, for posting a structured comment</param>
        /// <param name="commentName">Optional comment text, for posting a freetext comment</param>
        /// <param name="removeComment">Flag for whether the comment should be removed</param>
        /// <returns>Integer result for affected comments</returns>
        public int PostCommentByIdAsync(byte siteId, string patientId, int userId, Int32? commentId, string commentName = null, bool removeComment = false)
        {
            return _context.PostPatientCommentById(siteId, patientId, userId, commentId, commentName, removeComment).FirstOrDefault();
        }

        /// <summary>
        /// Sign a patient's chart
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>String. Null when signature was successful, otherwise non-null with error</returns>
        public async Task<string> SignChart(byte siteId, string patientId, User user)
        {
            var result = await _context.PatientDetails(siteId, patientId).ToListAsync();
            if (result != null && result.Count > 0)
            {
                var patient = GetPatient(result[0], user);
                return Signature.QueueNewChartForSigning(patient, user);
            }
            return "Patient selection not valid";
        }

        /// <summary>
        /// Get the AgeUnit for a patient's age
        /// </summary>
        /// <param name="au">Age unit identifier string</param>
        /// <returns>AgeUnit object</returns>
        private AgeUnit GetAgeUnit(string au)
        {
            return au.ToUpper() == AgeUtil.Constants.AGEUNIT_YEARS ? AgeUnit.Year :
                    au.ToUpper() == AgeUtil.Constants.AGEUNIT_MONTHS ? AgeUnit.Month :
                    au.ToUpper() == AgeUtil.Constants.AGEUNIT_WEEKS ? AgeUnit.Week :
                    AgeUnit.Day;
        }

        /// <summary>
        /// Convert a CommentResultType to a Comment
        /// </summary>
        /// <param name="c">CommentResultType object</param>
        /// <returns>Comment object</returns>
        private Comment GetComment(CommentResultType c)
        {
            return new Comment()
            {
                Text = c.Comment,
                CommentNumOnTrackingBoard = c.CommentNum,
                DateTime = Time.DateTimeFromString(c.Date),
                Style = new Style()
                {
                    ColorCode = c.ColorCode,
                    ColorName = c.ColorName,
                    ColorValues = { c.ColorVal1, c.ColorVal2 }
                },
                User = new MinimalUser()
                {
                    Id = c.User,
                    Initials = c.UserInit,
                    SiteId = c.SiteId,
                },
                Losecs = c.Losecs
            };
        }

        /// <summary>
        /// Get Disposition object for dispo code
        /// </summary>
        /// <param name="code">Dispo code</param>
        /// <param name="name">Dispo name</param>
        /// <returns>New Disposition object</returns>
        private Disposition GetDispoCode(string code, string name)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                return null;
            }
            return new Disposition
            {
                Name = name,
                Code = code
            };
        }
               
        /// <summary>
        /// Get Disposition object for dispo location
        /// </summary>
        /// <param name="loc">Dispo location</param>
        /// <param name="name">Dispo location name</param>
        /// <returns>New Disposition object</returns>
        private Disposition GetDispoLocation(string loc, string name)
        {
            if (String.IsNullOrWhiteSpace(loc))
            {
                return null;
            }
            return new Disposition
            {
                Name = name,
                Code = loc
            };
        }

        /// <summary>
        /// Get the Gender object for a particular gender identifier
        /// </summary>
        /// <param name="g">Gender identifier string</param>
        /// <returns>Gender object</returns>
        private Gender GetGender(string g)
        {
            return !string.IsNullOrWhiteSpace(g)
                ? ((g.ToUpper().Equals("M") || g.ToUpper().Equals("MALE")) ? Gender.Male : 
                  (g.ToUpper().Equals("F") || g.ToUpper().Equals("FEMALE")) ? Gender.Female :
                  (g.ToUpper().Equals("U") || g.ToUpper().Equals("UNKNOWN")) ? Gender.Unknown :
                  (g.ToUpper().Equals("O") || g.ToUpper().Equals("OTHER")) ? Gender.Other
                : Gender.Unknown)
                : Gender.Unknown;
        }

        /// <summary>
        /// Convert a PatientResultType object to a Patient
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <param name="user">User object</param>
        /// <returns>Patient object</returns>
        private Patient GetPatient(PatientResultType p, User user)
        {
            var newPat = new Patient
            {
                SiteId = p.SiteId,
                Ibex = p.Ibex,
                FirstName = p.FName,
                MiddleName = p.MName,
                LastName = p.LName,
                Suffix = p.Suffix,
                Department = p.Dept,
                Ward = p.Ward,
                Ward2 = p.Ward2,
                Bed = p.Bed,
                EnterpriseId = p.EnterpriseId,
                MedicalRecordNumber = p.MedRec,
                AcctNum = p.AcctNum,
                Ssn = p.Ssn,
                Readmit = (!string.IsNullOrWhiteSpace(p.Readmit) && p.Readmit.ToUpper() == "Y"),
                Demographics = new Demographics()
                {
                    Age = new Age()
                    {
                        Value = p.Age,
                        Unit = GetAgeUnit(p.AgeUnits),
                        DateOfBirth = Time.DateTimeOrNullFromString(p.Dob)
                    },
                    Ethnicity = new Ethnicity()
                    {
                        Code = p.EthnicityCode,
                        Name = p.EthnicityName
                    },
                    Gender = GetGender(p.Gender),
                    PreferredLanguage = new Language()
                    {
                        Code = p.LanguageCode,
                        Name = p.LanguageName
                    }
                },
                FirstDoctor = p.FirstDoctor,
                Providers = GetProviders(p),
                Urgency = GetUrgency(p),
                Complaint = GetComplaint(p, user),
                VitalIndicator = GetVitalIndicator(p),
                DispoCode = GetDispoCode(p.DispoCode, p.DispoCodeName),
                DispoLocation = GetDispoLocation(p.DispoLoc, p.DispoLocName),
                OrderIndicators = GetOrderIndicators(p),
                LOSMins = p.LOSMins,
                Height = p.Height,
                Weight = p.Weight,
                TimeSeen = Time.DateTimeOrNullFromString(p.doctor_seen)
            };

            newPat.DisplayName = newPat.GetName();

            return newPat;
        }

        /// <summary>
        /// Convert a PatientResultType object to a Patient for the MTB
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <param name="user">User object</param>
        /// <param name="blueCode">Blue code side parameter value</param>
        /// <param name="orderClassInfo">Dictionary of order class information</param>
        /// <returns>Patient object</returns>
        private Patient GetPatientForMTB(PatientResultType p, User user, string blueCode, Dictionary<string, Dictionary<string, string>> orderClassInfo)
        {
            var newPat = new Patient
            {
                SiteId = p.SiteId,
                Ibex = p.Ibex,
                FirstName = p.FName,
                LastName = p.LName,
                MiddleName = p.MName,
                Suffix = p.Suffix,
                Department = p.Dept,
                Ward = p.Ward,
                Ward2 = p.Ward2,
                Bed = p.Bed,
                Readmit = (!string.IsNullOrWhiteSpace(p.Readmit) && p.Readmit.ToUpper() == "Y"),
                Registration = GetRegistration(p, blueCode),
                Demographics = new Demographics
                {
                    Age = new Age
                    {
                        Value = p.Age,
                        Unit = GetAgeUnit(p.AgeUnits),
                        DateOfBirth = Time.DateTimeOrNullFromString(p.Dob)
                    },
                    Gender = GetGender(p.Gender),
                    Ethnicity = null,
                    PreferredLanguage = null,
                },
                Height = p.Height,
                Weight = p.Weight,
                Urgency = GetUrgency(p),
                Complaint = GetComplaint(p, user),
                VitalIndicator = GetVitalIndicator(p),
                OrderIndicators = GetOrderIndicators(p, orderClassInfo),
                DispoCode = GetDispoCode(p.DispoCode, p.DispoCodeName),
                DispoLocation = GetDispoLocation(p.DispoLoc, p.DispoLocName),
                FirstDoctor = p.FirstDoctor,
                Providers = GetProviders(p),
                LOSMins = p.LOSMins,
                TimeSeen = Time.DateTimeOrNullFromString(p.doctor_seen),
                //ContactInfo = null,
                Comments = null             // TODO: In the future this will need to be included (PulseCheck MTB).
            };

            newPat.DisplayName = newPat.GetName();

            return newPat;
        }

        /// <summary>
        /// Get the Complaint object for a PatientResultType, if the user has the necessary permissions.
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <param name="user">User object</param>
        /// <returns>Complaint object, or null if the user does not have permission</returns>
        private Complaint GetComplaint(PatientResultType p, User user)
        {
            if (user.HasAtLeastReadPermission(Permission.VIEW_COMPLAINT))
            {
                return new Complaint
                {
                    Name = p.Complaint,
                    Code = p.ComplaintCode
                };
            }

            return null;
        }

        /// <summary>
        /// Get the list of order indicators for a PatientResultType
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <returns>List of Indicator objects</returns>
        private List<Indicator> GetOrderIndicators(PatientResultType p, Dictionary<string, Dictionary<string, string>> orderColors = null)
        {
            var indicators = new List<Indicator>();

            if (!string.IsNullOrWhiteSpace(p.Ord27))
            {
                var name = "RESULT_NOT_VIEWED";
                var text = "Result not yet viewed";
                if (p.Ord27.Equals("R"))
                {
                    name = "ABNORMAL_RESULT_NOT_VIEWED";
                    text = "Abnormal result not yet viewed";
                } else if (p.Ord27.Equals("Z") || p.Ord27.Equals("C"))
                {
                    name = "CRITICAL_RESULT_NOT_VIEWED";
                    text = "Critical result not yet viewed";
                }
                indicators.Add(new Indicator
                {
                    Name = name,
                    Text = text
                });
            }

            if (p.Ord29.Equals("Y"))
            {
                indicators.Add(new Indicator
                {
                    Name = "ORDER_LATE",
                    Text = "Order is late in receiving the result"
                });
            }

            if (p.Ord47.Equals("Y"))
            {
                indicators.Add(new Indicator
                {
                    Name = "ORDER_OUTSTANDING",
                    Text = "Outstanding order"
                });
            }

            if (p.Ord21.Equals("Y"))
            {
                indicators.Add(new Indicator
                {
                    Name = "PATIENT_INDICATOR",
                    Text = "Patient Indicator"
                });
            }

            if (p.AllDone)
            {
                indicators.Add(new Indicator
                {
                    Name = "ALL_ORDERS_DONE",
                    Text = "All Orders Done"
                });
            }

            var orderDeptCol = new Dictionary<string, string>
            {
                { "X", p.Ord0 },
                { "L", p.Ord1 },
                { "R", p.Ord2 },
                { "E", p.Ord3 },
                { "N", p.Ord4 },
                { "C", p.Ord20 },
                { "G", p.Ord22 },
                { "D", p.Ord25 },
                { "P", p.Ord26 },
                { "M", p.Ord30 },
                { "A", p.Ord56 },
                { "Z", p.Ord46 }
            };

            foreach(var k in orderDeptCol.Keys)
            {
                if (string.IsNullOrWhiteSpace(orderDeptCol[k]) || (orderColors != null && !orderColors.ContainsKey(orderDeptCol[k])))
                {
                    continue;
                }

                var style = new Style();
                if (orderColors != null) {
                    var colorInfo = orderColors[orderDeptCol[k]];
                    var colorValues = new List<string> { colorInfo["Value1"] };
                    if (colorInfo["Value2"] != null)
                    {
                        colorValues.Add(colorInfo["Value2"]);
                    }
                    style.ColorCode = colorInfo["Name"];
                    style.ColorValues = colorValues;
                } else
                {
                    style = null;
                }
                indicators.Add(new Indicator {
                    Name = "ORDER_DEPT_" + k,
                    Text = k,
                    Style = style
                });
            }

            return indicators;
        }

        /// <summary>
        /// Get the First* providers that are only present in Encounter objects
        /// </summary>
        /// <param name="p">EncounterResultType object</param>
        private List<Object> GetEncounterProviders(EncounterResultType p)
        {
            return new List<Object>()
            {
                new MinimalProvider()
                {
                    Role = new StaffRole
                    {
                        Id = Constants.ID_FirstDoctor,
                        Description = Constants.Role_First_Doctor
                    },
                    User = p.FirstDoctor > 0 ? new MinimalUser()
                    {
                        Id = p.FirstDoctor,
                        Initials = p.FirstDoctorInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole
                    {
                        Id = Constants.ID_FirstDoctorExtender,
                        Description = Constants.Role_First_Doctor_Extender
                    },
                    User = p.FirstDrExtender > 0 ? new MinimalUser()
                    {
                        Id = p.FirstDrExtender,
                        Initials = p.FirstDrExtenderInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole
                    {
                        Id = Constants.ID_FirstResident,
                        Description = Constants.Role_First_Resident
                    },
                    User = p.FirstResident > 0 ? new MinimalUser()
                    {
                        Id = p.FirstResident,
                        Initials = p.FirstResidentInit
                    } : null
                },
            };
        }

        /// <summary>
        /// Get the list of providers for a PatientResultType
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <returns>List of MinimalProvider objects</returns>
        private List<Object> GetProviders(IResultTypeWithProviders p)
        {
            var providerSet = new List<Object>()
            {
                new MinimalProvider()
                {
                    Role = new StaffRole
                    {
                        Id = Constants.Id_Doctor,
                        Description = Constants.Role_Attending
                    },
                    User = p.Doctor > 0 ? new MinimalUser()
                    {
                        Id = p.Doctor,
                        Initials = p.DoctorInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole
                    {
                        Id = Constants.Id_Resident,
                        Description = Constants.Role_Resident
                    },
                    User = p.Resident > 0 ? new MinimalUser()
                    {
                        Id = p.Resident,
                        Initials = p.ResidentInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole {
                        Id = Constants.Id_Extender,
                        Description = Constants.Role_NurseExtender
                    },
                    User = p.Extender > 0 ? new MinimalUser()
                    {
                        Id = p.Extender,
                        Initials = p.ExtenderInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole {
                        Id = Constants.Id_DoctorExtender,
                        Description = Constants.Role_Extender
                    },
                    User = p.DrExtender > 0 ? new MinimalUser()
                    {
                        Id = p.DrExtender,
                        Initials = p.DrExtenderInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole {
                        Id = Constants.Id_PrimaryNurse,
                        Description = Constants.Role_PrimaryNurse
                    },
                    User = p.PrimaryNurse > 0 ? new MinimalUser()
                    {
                        Id = p.PrimaryNurse,
                        Initials = p.PrimaryNurseInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole {
                        Id = Constants.Id_CareCoordinator,
                        Description = Constants.Role_CareCoordinator
                    },
                    User = p.CareCoordinator > 0 ? new MinimalUser()
                    {
                        Id = p.CareCoordinator,
                        Initials = p.CareCoordinatorInit
                    } : null
                },
                new MinimalProvider()
                {
                    Role = new StaffRole {
                        Id = Constants.Id_Scribe,
                        Description = Constants.Role_Scribe
                    },
                    User = p.Scribe > 0 ? new MinimalUser()
                    {
                        Id = p.Scribe,
                        Initials = p.ScribeInit
                    } : null
                }
            };

            if (p is EncounterResultType)
            {
                providerSet.AddRange(GetEncounterProviders((EncounterResultType)p));
            }

            return providerSet;
        }

        /// <summary>
        /// Get the registration indicator for a patient
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <param name="blueCode">Blue Code org option value</param>
        /// <returns>Indicator object</returns>
        private Indicator GetRegistration(PatientResultType p, string blueCode)
        {
            var registrationName = "MR and Acct # present";
            var registrationStyle = new Style
            {
                ColorName = "Green",
                ColorCode = "G",
                ColorValues = new List<string> { "#60D760", "##CCCCCC" }
            };

            if (string.IsNullOrWhiteSpace(p.MedRec) || string.IsNullOrWhiteSpace(p.AcctNum))
            {
                registrationName = "No MR/Acct #";
                registrationStyle.ColorName = "Red";
                registrationStyle.ColorCode = "R";
                registrationStyle.ColorValues = new List<string> { "#FF453E", "#666666" };
            }
            else if (blueCode.Equals("Z") && !string.IsNullOrWhiteSpace(p.Zip))
            {
                registrationName = "Postal code present";
                registrationStyle.ColorName = "Blue";
                registrationStyle.ColorCode = "B";
                registrationStyle.ColorValues = new List<string> { "#64AAF5", "#64AAF5" };
            }
            else if (blueCode.Equals("P") && !string.IsNullOrWhiteSpace(p.Paycode))
            {
                registrationName = "Payment code present";
                registrationStyle.ColorName = "Blue";
                registrationStyle.ColorCode = "B";
                registrationStyle.ColorValues = new List<string> { "#64AAF5", "#64AAF5" };
            }

            return new Indicator
            {
                Name = "REGISTRATION_STATUS",
                Text = registrationName,
                Style = registrationStyle
            };
        }

        /// <summary>
        /// Get the Urgency object for a PatientResultType
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <returns>Urgency object</returns>
        private Urgency GetUrgency(PatientResultType p)
        {
            return new Urgency()
            {
                Name = p.UrgName,
                Eun = p.Eun,
                Style = new Style()
                {
                    ColorCode = p.UrgColorCode,
                    ColorName = p.UrgColorName,
                    ColorValues = { p.UrgColorVal1, p.UrgColorVal2 }
                }
            };
        }

        /// <summary>
        /// Get the vital indicator for a PatientResultType
        /// </summary>
        /// <param name="p">PatientResultType object</param>
        /// <returns>VitalIndicator object</returns>
        private VitalIndicator GetVitalIndicator(PatientResultType p)
        {
            if (String.IsNullOrEmpty(p.VitalIndicator))
            {
                return null;
            }

            return new VitalIndicator()
            {
                Text = p.VitalIndicator,
                Name = (
                    (p.VitalColorCode.Equals(VitalSigns.Constants.PANIC_HIGH_CODE) || p.VitalColorCode.Equals(VitalSigns.Constants.PANIC_LOW_CODE)) ? "VS Severely out of range" : 
                    (p.VitalColorCode.Equals(VitalSigns.Constants.WARN_HIGH_CODE) || p.VitalColorCode.Equals(VitalSigns.Constants.WARN_LOW_CODE)) ? "VS mildly out of range" : 
                    ""
                ),
                Style = new Style(VitalSigns.GetVitalStyleInfo(p.VitalColorCode))
            };
        }
        
        /// <summary>
        /// Perform data expansion on a particular Patient object
        /// </summary>
        /// <param name="patient">Patient object</param>
        /// <param name="siteId">Site identifier for patient</param>
        /// <param name="user">User object</param>
        /// <param name="expand">Expand parameter</param>
        /// <returns></returns>
        private async Task<Patient> ExpandPatient(Patient patient, byte siteId, User user, string expand)
        {
            List<Patient> p = new List<Patient>()
            {
                patient
            };

            p = await ExpandPatients(p, siteId, user, expand);

            return p.ToList().First();
        }

        /// <summary>
        /// Perform data expansion on a list of Patient objects
        /// </summary>
        /// <param name="patients">List of Patient objects</param>
        /// <param name="siteId">Site identifier for patients</param>
        /// <param name="user">User object</param>
        /// <param name="expand">Expand parameter</param>
        /// <returns></returns>
        private async Task<List<Patient>> ExpandPatients(List<Patient> patients, byte siteId, User user, string expand)
        {
            if (String.IsNullOrWhiteSpace(expand))
            {
                return patients;
            }
            expand = expand.ToLower();

            // Expansion could be performed on a patient list or a single patient, so both the 'patients.' 
            // and 'patient.' prefixes would make sense. To handle this, convert the plural to the singular
            // and only match against the singular.
            expand = expand.Replace("patients.", "patient.");

            var expandAll = (expand.Equals(Constants.Expando.EXPAND_ALL));

            var includeComments = user.HasAtLeastReadPermission(Permission.COMMENTS);
            var includeEncounters = user.HasAtLeastReadPermission(Permission.VISIT_HISTORY);

            foreach (var p in patients)
            {
                if (expand.Contains(Constants.Expando.EXPAND_VITALSIGNS) || expandAll)
                {
                    var vitals = _context.FullPatientVitals(siteId, p, user);
                    await MeaningfulUse.LogAccess(user, p.Ibex, "FLOWSHEET");
                    p.VitalSigns = vitals;
                }

                if (expand.Contains(Constants.Expando.EXPAND_DIGITALSIGNATURES) || expandAll)
                {
                    var signatureInfo = await _context.PatientDigitalSignaturesAsync(siteId, p.Ibex, user);
                    if (p.Chart == null)
                    {
                        p.Chart = new Chart();
                    }
                    p.Chart.SignableEvents = signatureInfo.SignableEvents; 
                    p.Chart.DigitalSignatures = signatureInfo.DigitalSignatures;
                }

                if (expand.Contains(Constants.Expando.EXPAND_RACES) || expandAll)
                {
                    var races = await _context.PatientRaces(siteId, p.Ibex).ToListAsync();
                    p.Demographics.Races = races.Select(r => new Race()
                    {
                        Name = r.Name,
                        Code = r.Code
                    }).ToList();
                }

                if (expand.Contains(Constants.Expando.EXPAND_COMMENTS) || expandAll)
                {
                    if (includeComments)
                    {
                        var comments = await _context.PatientComments(siteId, p.Ibex, true).ToListAsync();
                        p.Comments = comments.Select(c => GetComment(c)).ToList();
                    }
                }

                if ((expand.Contains(Constants.Expando.EXPAND_ENCOUNTERS) || expandAll) && includeEncounters)
                {
                    p.Encounters = await GetPatientEncounters(siteId, p.Ibex, user);
                    if (includeComments)
                    {
                        if (expand.Contains(Constants.Expando.EXPAND_ENCOUNTER_COMMENTS) || expandAll)
                        {
                            foreach (var e in p.Encounters)
                            {
                                var encounterComments = await _context.PatientComments(((MinimalSite)e.Site).Id, e.Ibex, false).ToListAsync();
                                e.Comments = encounterComments.Select(c => GetComment(c)).ToList();
                            }
                        }

                        if (expand.Contains(Constants.Expando.EXPAND_ENCOUNTER_DIGITALSIGNATURES) || expandAll)
                        {
                            foreach (var e in p.Encounters)
                            {
                                var encounterDigitalSignatureInfo = await _context.PatientDigitalSignaturesAsync(((MinimalSite)e.Site).Id, e.Ibex, user);
                                if (e.Chart == null)
                                {
                                    e.Chart = new Chart();
                                }
                                e.Chart.DigitalSignatures = encounterDigitalSignatureInfo.DigitalSignatures;
                                e.Chart.SignableEvents = encounterDigitalSignatureInfo.SignableEvents;
                            }
                        }
                    }
                }
            }

            return patients;
        }
    }
}
