using AutoMapper;
using ProyectoExamenU2.Databases.LogsDataBase.Entities;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Balance;
using ProyectoExamenU2.Dtos.Logs;

namespace ProyectoExamenU2.Helpers
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            MapsForAccounts();
            MapsForVBalances();

        }

        private void MapsForLogas()
        {
            CreateMap<LogErrorEntity, LogErrorDto>();

            CreateMap<LogDetailEntity, LogDetailDto>();
            // Este trae el detalle y el Error 
            CreateMap<LogEntity, LogDto>()
                .ForMember(dest => dest.Detail, opt => opt.MapFrom(src => src.Detail)) 
                .ForMember(dest => dest.Error, opt => opt.MapFrom(src => src.Error)); 
        }

        private void MapsForVBalances()
        {
            CreateMap<BalanceEntity, BalanceDto>()
            .ForMember(dest => dest.AccountCatalog, opt => opt.MapFrom(src => src.AccountCatalog))
            .ForMember(dest => dest.BalanceAmount, opt => opt.MapFrom(src => src.BalanceAmount))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date));
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
