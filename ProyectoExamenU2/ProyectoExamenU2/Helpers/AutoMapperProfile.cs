using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using ProyectoExamenU2.Databases.LogsDataBase.Entities;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Balance;
using ProyectoExamenU2.Dtos.Journal;
using ProyectoExamenU2.Dtos.Journal.DtosHelper;
using ProyectoExamenU2.Dtos.Logs;

namespace ProyectoExamenU2.Helpers
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            MapsForAccounts();
            MapsForVBalances();
            MapsForLogas();
            MapsForJournalEntrys();
        }
        private void MapsForJournalEntrys()
        {
            //CreateMap<JournalEntryEntity, JournalDto>()
            //    .ForMember(dest => dest.JournalEntryDetails , opt => opt.Ignore());
            CreateMap<JournalEntryCreateDto , JournalEntryEntity>();
            CreateMap<JournalEntryDetailDto , JournalEntryDetailEntity>();
            CreateMap<AccountEntry, JournalEntryDetailEntity>();
            CreateMap<JournalEntryEntity, JournalDto>()
                .ForMember(dest => dest.JournalEntryDetails, opt => opt.MapFrom(src => src.JournalEntryDetails));
                //.ForMember(dest => dest., opt => opt.Ignore())
                //.ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore());
            CreateMap<JournalEntryDetailEntity, JournalEntryDetailDto>();


        }
        private void MapsForLogas()
        {
            //Logs
            CreateMap<LogCreateDto, LogEntity>();

            CreateMap<LogErrorEntity, LogErrorDto>()
           .ForMember(dest => dest.StackTrace, opt => opt.Ignore()); // Ignorar StackTrace

            CreateMap<LogErrorCreateDto, LogErrorEntity>();
            CreateMap<LogDetailCreateDto, LogDetailEntity>();

            // Detalles
            CreateMap<LogDetailDto, LogDetailEntity>();
            CreateMap<LogDetailEntity, LogDetailDto>()
                .ForMember(dest => dest.OldValues, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.OldValues) ? null : JsonConvert.DeserializeObject<dynamic>(src.OldValues)))
                .ForMember(dest => dest.NewValues, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.NewValues) ? null : JsonConvert.DeserializeObject<dynamic>(src.NewValues)));


            // entidad a Dto
            CreateMap<LogEntity, LogDto>()
                .ForMember(dest => dest.Detail, opt => opt.MapFrom(src => src.Detail))
                .ForMember(dest => dest.Error, opt => opt.MapFrom(src => src.Error));
            CreateMap<LogErrorEntity, LogErrorDto>()
           .ForMember(dest => dest.StackTrace, opt => opt.Ignore()); // Ignorar StackTrace

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
