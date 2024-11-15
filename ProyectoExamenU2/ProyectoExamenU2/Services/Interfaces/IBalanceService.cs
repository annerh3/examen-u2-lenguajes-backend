using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Balance;
using ProyectoExamenU2.Dtos.Common;

namespace ProyectoExamenU2.Services.Interfaces
{
    public interface IBalanceService
    {
        Task<ResponseDto<BalanceDto>> CreateInitBalance(Guid id);

        //Task<ResponseDto<BalanceDto>> updateBalance(BalanceDto balance);

    }
}
