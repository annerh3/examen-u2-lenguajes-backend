using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Common;

namespace ProyectoExamenU2.Services.Interfaces
{
    public interface IAccountCatalogService
    {
        Task<ResponseDto<AccountDto>> CreateAcoountAsync(AccountCreateDto dto);
        Task<ResponseDto<AccountDto>> DowAccountByIdAsync(Guid id);
        Task<ResponseDto<AccountDto>> EditAccountByIdAsync(AccountEditDto dto, Guid id);
        Task<ResponseDto<List<AccountDto>>> GetJustChildAccountListAsync();

        Task<ResponseDto<List<AccountDto>>> GetJustChildAccountInactiveListAsync();
        Task<ResponseDto<AccountDto>> GetAccountByIdAsync(Guid id);

    }
}
