using AutoMapper;

namespace PulseCheck.Archive.Logic.Bindings
{
    public class AutoMapperRegistration : Profile
    {
        public void RegisterMappings()
        {
            Mapper.Initialize(cfg => { cfg.AddProfile<AutoMapperRegistration>(); });
        }

        public AutoMapperRegistration()
        {
            //CreateMap<Route, Data.AccessObject.Route>().ReverseMap();
            //CreateMap<Product, Data.AccessObject.Product>().ReverseMap();
            //CreateMap<Procedure, Data.AccessObject.Procedure>()
            //    .ForMember(dest => dest.QcprImportId, opt => opt.MapFrom(src => src.ImportArchiveId))
            //    .ReverseMap();

            //CreateMap<Product, GetProductsResponse>().ReverseMap();
            //CreateMap<Procedure, GetProdceduresResponse>().ReverseMap();
        }
    }
}
