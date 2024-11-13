using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.Balance;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Helpers;
using ProyectoExamenU2.Services.Interfaces;

namespace ProyectoExamenU2.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly ProyectoExamenU2Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<IBalanceService> _logger;
        private readonly IConfiguration _configuration;

        public BalanceService(
            ProyectoExamenU2Context context,
            IMapper mapper,
            ILogger<IBalanceService> logger,
            IConfiguration configuration

            )
        {
            this._context = context;
            this._mapper = mapper;
            this._logger = logger;
            this._configuration = configuration;
        }


        public async Task<ResponseDto<BalanceDto>> CreateInitBalance(Guid id)
        {
            var existingBalance = await _context.Balances.FirstOrDefaultAsync(b => b.AccountCatalogId == id);

            if (existingBalance == null)
            {
                var newBalance = new BalanceEntity
                {
                    AccountCatalogId = id,
                    BalanceAmount = 0,
                    Date = DateTime.UtcNow
                };

                _context.Balances.Add(newBalance);
                await _context.SaveChangesAsync();

                var balanceDto = _mapper.Map<BalanceDto>(newBalance);
                return ResponseHelper.ResponseSuccess(
                    CodesConstant.CREATED, "Balance inicial creado exitosamente.", balanceDto
                );
            }

            var entityBalance = _mapper.Map<BalanceDto>(existingBalance);
            return ResponseHelper.ResponseSuccess(
                CodesConstant.OK, "El balance ya existe", entityBalance
            );
        }
    }
}
