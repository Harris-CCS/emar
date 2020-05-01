using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using PulseCheck.QCPR.Domain.Data;
using PulseCheck.QCPR.Data;

namespace PulseCheck.QCPR.Logic.Bindings
{
    public class AutoMapperRegistration : Profile
    {
        public void RegisterMappings()
        {
            Mapper.Initialize(cfg => { cfg.AddProfile<AutoMapperRegistration>(); });
        }

        public AutoMapperRegistration()
        {
            CreateMap<Route, Data.AccessObject.Route>().ReverseMap();
            CreateMap<Product, Data.AccessObject.Product>().ReverseMap();
            CreateMap<Procedure, Data.AccessObject.Procedure>()
                .ForMember(dest => dest.QcprImportId, opt => opt.MapFrom(src => src.ImportArchiveId))
                .ReverseMap();

            CreateMap<GetProductsResponse, Data.AccessObject.Product>()
                .ForMember(dest => dest.QcprProductId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();
            CreateMap<GetProceduresResponse, Data.AccessObject.Procedure>()
                .ForMember(dest => dest.QcprProcedureId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();
        }
    }
}
