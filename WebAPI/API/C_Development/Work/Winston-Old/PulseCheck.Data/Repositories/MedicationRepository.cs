using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.IRepository;

namespace PulseCheck.Data.Repositories
{
    /// <summary>
    /// Medication repository
    /// </summary>
    public class MedicationRepository : BaseRepository, IMedicationRepository
    {
        /// <summary>
        /// Default empty constructor
        /// </summary>
        /// <param name="context">DB context</param>
        public MedicationRepository(IbexContext context) : base(context)
        {

        }

        /// <summary>
        /// Get a list of medications for a patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>List of Medication objects</returns>
        public async Task<List<Medication>> GetMedicationsByPatientIdAsync(byte siteId, string patientId)
        {
            return await _context.Medications
                .Where(r => r.Ibex.Equals(patientId))
                .Where(r => r.Site == siteId)
                .Include(i => i.Components).ToListAsync();
        }

        /// <summary>
        /// Get a medication from its DB identifier
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="medId">Medication DB identifier</param>
        /// <returns>Medication object</returns>
        public async Task<Medication> GetMedicationByIdAsync(byte siteId, string patientId, int medId)
        {
            return await _context.Medications
                .Where(r => r.Ibex.Equals(patientId))
                .Where(r => r.Site == siteId)
                .Where(r => r.Id == medId)
                .Include(i => i.Components).FirstAsync();
        }

        /// <summary>
        /// Get a medication from its losecs identifier
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="losecs">Medication losecs identifier</param>
        /// <returns>Medication object</returns>
        public async Task<Medication> GetMedicationByLosecsAsync(byte siteId, string patientId, int losecs)
        {
            return await _context.Medications
                .Where(r => r.Ibex.Equals(patientId))
                .Where(r => r.Site == siteId)
                .Where(r => r.Losecs == losecs)
                .Include(i => i.Components).FirstAsync();
        }

        /// <summary>
        /// Save a medication to the database
        /// </summary>
        /// <param name="med">Medication object to save</param>
        /// <returns>Result from SaveChangesAsync on DB context</returns>
        public async Task<int> Save(Medication med)
        {
            _context.Entry(med).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }
    }
}
