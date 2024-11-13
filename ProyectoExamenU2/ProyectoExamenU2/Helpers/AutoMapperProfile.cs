using AutoMapper;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.AccountCatalog;

namespace ProyectoExamenU2.Helpers
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            MapsForAccounts();

        }


        private void MapsForAccounts()
        {
            // Mapear de AccountCreateDto a AccountEntity para crear una cuenta
            CreateMap<AccountCreateDto, AccountCatalogEntity>()
                .ForMember(dest => dest.ParentAccount, opt => opt.Ignore()); // Ignorar ParentAccount si solo se manda el ID

            // Mapear de AccountEditDto a AccountCatalogEntity para editar una cuenta
            CreateMap<AccountEditDto, AccountCatalogEntity>()
                .ForMember(dest => dest.ParentAccount, opt => opt.Ignore());

            // Mapear de AccountCatalogEntity a AccountDto para visualizar una cuenta
            //CreateMap<AccountCatalogEntity, AccountDto>()
            //     .ForMember(dest => dest.ParentAccount, opt => opt.MapFrom(src => src.ParentAccount))
            //     .ForMember(dest => dest.ChildAccounts, opt => opt.MapFrom(src => src.ChildAccounts));
            CreateMap<AccountCatalogEntity, AccountDto>()
                 .ForMember(dest => dest.ParentAccount, opt => opt.MapFrom(src => src.ParentAccount))
                 .ForMember(dest => dest.ChildAccounts, opt => opt.MapFrom(src => src.ChildAccounts));

            // para hojos
            CreateMap<AccountCatalogEntity, ChildAccountDto>()
                .ForMember(dest => dest.FullCode, opt => opt.MapFrom(src => src.PreCode + src.Code))
                .ForMember(dest => dest.ChildAccounts, opt => opt.MapFrom(src => src.ChildAccounts));


        }
    }
}
