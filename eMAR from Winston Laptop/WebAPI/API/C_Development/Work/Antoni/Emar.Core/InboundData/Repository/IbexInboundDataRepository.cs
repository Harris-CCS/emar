using Emar.Core.InboundData.Model;
using Emar.Data;
using Emar.Data.IbexEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Core.InboundData.Repository
{
    public class IbexInboundDataRepository : IIbexInboundDataRepository
    {
        private readonly IbexContext _context;
        private readonly ILogger<IbexInboundDataRepository> _logger;

        public IbexInboundDataRepository(IbexContext context, ILogger<IbexInboundDataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public EmarUpdateQueueMaintenance GetNextQueueRecordToProcess(
            ref NextQueueRecordToProcessDto nextQueueRecordToProcessDto)
        {
            List<EmarUpdateQueueMaintenance> result;

            if (nextQueueRecordToProcessDto == null)
                result = _context.EmarUpdateQueueMaintenances
                    .FromSqlInterpolated($"EXEC dbo.emar_update_queue_maintenance").ToList();
            else
            {
                var highId = nextQueueRecordToProcessDto.HighestQueueIdWhenQuerying;
                var type = nextQueueRecordToProcessDto.RecordType.ToString();
                var exId = nextQueueRecordToProcessDto.RecordExternalId;
                result = _context.EmarUpdateQueueMaintenances
                    .FromSqlInterpolated($"EXEC dbo.emar_update_queue_maintenance {highId}, {type}, {exId}")
                    .ToList();
            }

            if (result.Count == 0)
                return null;
            return result[0];
        }

        public EmarUsersRetrieveView GetUser(string externalId)
        {
            if (!int.TryParse(externalId, out int id))
            {
                _logger.LogError($"Found [external_id] in [emar_update_queue] for [entity] = 'users' ({externalId}) which was not an integer.");
                return null;
            }

            var usersRetrieve = _context.EmarUsersRetrieveViews.FirstOrDefault(u => u.Id == id);

            return  usersRetrieve;
        }

        public EmarPatientsRetrieveView GetPatient(string externalId)
        {
            // Parse the External ID (site/ibex)
            var idParts = externalId.Split("|");
            if (idParts.Length != 2)
            {
                _logger.LogError($"Found [external_id] in [emar_update_queue] for [entity] = 'patients' ({externalId}) which was not parseable into 2 parts (site and ibex).");
                return null;
            }
            if (!int.TryParse(idParts[0], out int site))
            {
                _logger.LogError(
                    $"Found [external_id] in [emar_update_queue] for [entity] = 'patients' ({externalId}) which had a first part (site) that was not parseable into an integer.");
                return null;
            }
            if (!long.TryParse(idParts[1], out long temp))
            {
                _logger.LogError(
                    $"Found [external_id] in [emar_update_queue] for [entity] = 'patients' ({externalId}) which had a second part (ibex) that was not parseable into a long.");
                return null;
            }
            var ibex = idParts[1];

            throw new NotImplementedException("IbexInboundDataRepository.GetPatient()");
            //var patientsRetrieve = _context.EmarPatientsRetrieveViews.FirstOrDefault(u => u.Site == site && u.Ibex == ibex);

            //return patientsRetrieve;
        }
    }
}
