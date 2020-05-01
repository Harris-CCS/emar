using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using DomainModel;
using Interfaces.Repository;
using PulseCheck.Utilities;

namespace Data.Repositories
{
    public class SiteRepository : BaseRepository, ISiteRepository
    {
        public SiteRepository(IbexContext context) : base(context)
        {

        }

        /// <summary>
        /// Get a list of sites
        /// </summary>
        /// <returns>Site objects</returns>
        public async Task<IEnumerable<Site>> GetSitesAsync()
        {
            var result = await _context.Sites.ToListAsync();

            //Set the Status object
            result.ForEach(x => x.Status = Status.GetStatusByCode(x.Status.Code));

            return result;
        }

        /// <summary>
        /// Get a single site by id
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <returns>Site object</returns>
        public async Task<Site> GetSiteByIdAsync(byte id)
        {
            return await _context.Sites.FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Get the comments defined for a site
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <returns>List of comments</returns>
        public async Task<List<SiteElement>> GetCommentsBySiteIdAsync(byte id)
        {
            var result = await _context.CommentsBySiteId(id).ToListAsync();
            return result.Select(s => new SiteElement()
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Style = new Style()
                {
                    ColorCode = s.ColorCode,
                    ColorName = s.ColorName,
                    ColorValues = { s.ColorVal1, s.ColorVal2 }
                }
            }).ToList();
        }

        /// <summary>
        /// Get metadata used when ordering medications (route options, schedule options, etc.)
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <returns>Dictionary of MetaData objects</returns>
        public async Task<Dictionary<string, MetaData>> GetMedMetaDataBySiteIdAsync(byte id)
        {
            var med = new Medication(id);
            var dataItems = med.GetDataItems();
            var meta = new Dictionary<string, MetaData>();
            var result = await _context.GetSiteMedMetaData(id).ToListAsync();
            var sortedResults = result.OrderBy(o => o.Type).ThenBy(o => o.Misc2).ThenBy(o => o.Name).ToList();
            foreach(var r in sortedResults)
            {
                var type = r.Type.Trim();
                var i = r.Id.Trim();
                var name = r.Name.Trim();
                var kName = "";
                var description = "";

                // Allergy reaction override
                if (type.Equals("A"))
                {
                    kName = "algoverride";
                    description = "Allergy reaction override";

                // Medication interaction override
                } else if (type.Equals("M"))
                {
                    kName = "medoverride";
                    description = "Medication interaction override";

                // Route options
                } else if (type.Equals("AC"))
                {
                    kName = "routeopts";
                    description = "Medication route options";

                // Unit options
                } else if (type.Equals("BE"))
                {
                    kName = "unitopts";
                    description = "Medication unit options";

                // Schedule options
                } else if (type.Equals("BS"))
                {
                    kName = "scheduleopts";
                    description = "Medication schedule options";

                // Med service options
                } else if (type.Equals("SO"))
                {
                    kName = "serviceopts";
                    description = "Medication Service options";
                
                // Ordering physician options
                } else if (type.Equals("OP"))
                {
                    kName = "orderingphysicianopts";
                    description = "Ordering Physician options";

                // Ordering-only physician options
                } else if (type.Equals("OO"))
                {
                    kName = "orderingonlyphysicianopts";
                    description = "Ordering Only Physician options";                
                } else if (type.Equals("IN"))
                {
                    kName = "indicationopts";
                    description = "Medication Indication options";
                }


                if (!string.IsNullOrEmpty(kName))
                {
                    if (!meta.ContainsKey(kName))
                    {
                        meta.Add(kName, new MetaData
                        {
                            Name = kName,
                            Description = description
                        });
                    }
                    meta[kName].AddOption(name, i);
                }
            }

            if (meta.ContainsKey("serviceopts"))
            {
                meta["serviceopts"].Options = meta["serviceopts"].Options.OrderBy(o => o.Value).ToList();
            }

            meta["dataitems"] = new MetaData
            {
                Name = "Data Items",
                Description = "Data Items",
            };
            foreach (var k in dataItems.Keys)
            {
                meta["dataitems"].AddOption(k, dataItems[k]);
            }

            return meta;
        }

        /// <summary>
        /// Get the medication pathways/groups for a site
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <returns>List of Group objects</returns>
        public async Task<List<DomainModel.Group>> GetMedPathwaysBySiteIdAsync(byte id)
        {
            var result = await _context.GetSiteGroups(id).ToListAsync();
            return result.Select(g => new DomainModel.Group
            {
                Name = g.Name,
                Num = g.Num,
                Type = g.Type,
                AltCode = g.AltCode,
                Style = new Style()
                {
                    ColorCode = g.ColorCode,
                    ColorName = g.ColorName,
                    ColorValues = { g.ColorVal1, g.ColorVal2 }
                }
            }).OrderBy(o => o.AltCode).ToList();
        }

        /// <summary>
        /// Get a single order pathway for a site
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="pathwayNum">Pathway number/identifier</param>
        /// <returns>ClinicalPathway object</returns>
        public async Task<ClinicalPathway> GetOrderPathwayByIdAsync(byte siteId, int pathwayNum)
        {
            var result = await _context.GetSitePathways(siteId, pathwayNum).FirstOrDefaultAsync();
            return new ClinicalPathway(result.Num, result.Name, "A");
        }

        /// <summary>
        /// Get the order pathways for a site
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <returns>List of Group objects</returns>
        public async Task<List<DomainModel.Group>> GetOrderPathwaysBySiteIdAsync(byte id) {
            var result = await _context.GetSitePathways(id).ToListAsync();
            return result.Select(g => new DomainModel.Group
            {
                Name = g.Name,
                Num = g.Num,
                Type = g.Type,
                AltCode = g.AltCode,
                Style = new Style()
                {
                    ColorCode = g.ColorCode,
                    ColorName = g.ColorName,
                    ColorValues = { g.ColorVal1, g.ColorVal2 }
                }
            }).OrderBy(o => o.AltCode).ToList();
        }

        /// <summary>
        /// Search for matching clinical pathways by name
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <param name="name">Clinical pathway name</param>
        /// <param name="limit">Results limit</param>
        /// <returns></returns>
        public async Task<List<ClinicalPathway>> SearchClinicalPathwaysBySiteIdAsync(byte id, string name, int limit)
        {
            var result = await _context.SearchClinicalPathways(id, name, limit).ToListAsync();
            return result.Select(c => new ClinicalPathway(c.Num, c.Name, c.Status)).OrderBy(o => o.Name).ToList();
        }

        /// <summary>
        /// Search for matching medications by name
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <param name="name">Clinical pathway name</param>
        /// <param name="limit">Results limit</param>
        /// <returns></returns>
        public async Task<List<string>> SearchMedicationBrandsBySiteIdAsync(byte id, string brand, int limit)
        {
            var result = await _context.SearchSiteMedications(id, brand, limit).Select(x => x.Brand).ToListAsync();
            return result;
        }

        /// <summary>
        /// Search for matching medications by name
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <param name="name">Clinical pathway name</param>
        /// <param name="limit">Results limit</param>
        /// <returns></returns>
        public async Task<List<Service>> SearchOrdersBySiteIdAsync(byte id, string name, int limit, int userId)
        {
            var result = await _context.SearchSiteOrders(id, name, limit, userId).Select(s => new Service
            {
                Name = s.Name,
                Code = s.Code.Trim(),
                InterfaceType = s.Face,
                MaxQuantity = s.MaxQty,
                Type = s.SvcType,
                Number = s.Number,
                IsUserFavorite = s.IsUserFavorite
            }).ToListAsync();
            return result;
        }

        /// <summary>
        /// Get a list of available locations within the site, and optionally within a department
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <param name="dept">Optional department name</param>
        /// <returns>List of Location objects</returns>
        public async Task<List<Location>> GetAvailableLocationsBySiteIdAsync(byte id, string dept = null)
        {
            var result = await _context.AvailableLocationsBySiteId(id, dept).ToListAsync();
            return result.OrderBy(o => o.GroupNum).ThenBy(o => o.Dept).ThenBy(o => o.Name).Select(l => new Location()
            {
                GroupType = l.GroupType,
                Department = l.Dept,
                Ward = l.Ward,
                Bed = l.Bed,
                Id = l.Id,
                Name = l.Name,
                Patient = null
            }).ToList();
        }

        /// <summary>
        /// Get a list of share locations within the site, and optionally within a department
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <param name="dept">Optional department name</param>
        /// <returns>List of Location objects</returns>
        public async Task<List<Location>> GetShareLocationsBySiteIdAsync(byte id, string dept = null)
        {
            var result = await _context.ShareLocationsBySiteId(id, dept).ToListAsync();
            return result.OrderBy(o => o.Dept).ThenBy(o => o.Ward).ThenBy(o => o.Bed).Select(l => new Location()
            {
                Department = l.Dept,
                Ward = l.Ward,
                Bed = l.Bed,
                Id = l.Id,
                Name = l.Bed,
                Patient = new Location.MinimallyIdentifiedPatient
                {
                    Ibex = l.Ibex,
                    LastName = l.LName,
                    FirstName = l.FName,
                    MiddleName = l.MName,
                    Suffix = l.Suffix
                }
            }).ToList();
        }

        /// <summary>
        /// Get the signup information for a site, rendered for the requesting user
        /// </summary>
        /// <param name="id">Site identifier</param>
        /// <param name="user">User object</param>
        /// <returns>Signup strings, raw and rendered</returns>
        public async Task<Dictionary<string, string>> GetSignupInfo(byte id, User user)
        {
            var result = await _context.GetSignupInfo(id).FirstAsync();
            var ret = new Dictionary<string, string>()
            {
                { "SeenTextRaw_" + Constants.Id_Doctor, result.DoctorSeenText },
                { "SeenTextRaw_" + Constants.Id_DoctorExtender, result.DoctorExtenderSeenText },
                { "SeenTextRaw_" + Constants.Id_Resident, result.ResidentSeenText }
            };

            var currentDateTime = (new Time(id)).LongDateTime();

            var keyList = ret.Keys.ToArray();
            foreach(string k in keyList)
            {
                var newKey = k.Replace("Raw_", "Rendered_");
                var value = ret[k];
                ret.Add(newKey, EMR.FormatSeenTimeInfo(value, user, currentDateTime));
            }

            return ret;
        }
    }
}
