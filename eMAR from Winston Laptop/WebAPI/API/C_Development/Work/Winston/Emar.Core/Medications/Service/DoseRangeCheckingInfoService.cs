using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Medications.Repository;

namespace Emar.Core.Medications.Service
{
    public class DoseRangeCheckingInfoService : IDoseRangeCheckingInfoService
    {
        private IDoseRangeCheckingInfoRepository _doseRangeCheckingInfoRepository;

        public DoseRangeCheckingInfoService(IDoseRangeCheckingInfoRepository doseRangeCheckingInfoRepository)
        {
            _doseRangeCheckingInfoRepository = doseRangeCheckingInfoRepository;
        }

        public IEnumerable<DoseRangeCheckingInfoDto> DoseRangeCheckInfos(int medid)
        {
            //List of entities.
            var infos = _doseRangeCheckingInfoRepository.RetrieveDoseRangeCheckingInfo(medid);

            //List of DTO objects.
            List<DoseRangeCheckingInfoDto> infoDtos = new List<DoseRangeCheckingInfoDto>();

            //For each entity in the list, map it to a DTO object and add to the DTO list.
            foreach (var info in infos)
            {
                infoDtos.Add(DoseRangeCheckingInfoMapper.MapDoseRangeCheckingInfo(info));
            }
            
            //Return.
            return infoDtos;
        }
    }
}