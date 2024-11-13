using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.Balance;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Dtos.Logs;
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
        private readonly ILoggerDBService _loggerDB;
        private readonly IAuditService _auditService;

        public BalanceService(
            ProyectoExamenU2Context context,
            IMapper mapper,
            ILogger<IBalanceService> logger,
            IConfiguration configuration,
            ILoggerDBService loggerDB,
            IAuditService auditService

            )
        {
            this._context = context;
            this._mapper = mapper;
            this._logger = logger;
            this._configuration = configuration;
            this._loggerDB = loggerDB;
            this._auditService = auditService;

        }


        public async Task<ResponseDto<BalanceDto>> CreateInitBalance(Guid id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
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

                        // Log de la transaccion
                        // se tiene que crear el log de cambios 
                        var UserId = new Guid (_auditService.GetUserId());
                        var log = new LogCreateDto
                        {
                            UserId = UserId,
                            ActionType = null,
                            Status = null,
                            Detail = null,
                            Error =null,
    };
                        await _loggerDB.LogCreateActionAsync(log);

                        // Confirma la transacción solo si todo ha sido exitoso
                        await transaction.CommitAsync();

                        var balanceDto = _mapper.Map<BalanceDto>(newBalance);
                        return ResponseHelper.ResponseSuccess(
                            CodesConstant.CREATED, "Balance inicial creado exitosamente.", balanceDto
                        );
                    }

                    var entityBalance = _mapper.Map<BalanceDto>(existingBalance);

                    // Si no se necesita crear un nuevo balance, de todos modos se confirma la transacción.
                    await transaction.CommitAsync();

                    return ResponseHelper.ResponseSuccess(
                        CodesConstant.OK, "El balance ya existe", entityBalance
                    );
                }
                catch (Exception ex)
                {
                    // Si ocurre un error, se hace rollback de la transacción
                    await transaction.RollbackAsync();
                    _logger.LogError($"Error al crear el balance inicial para el ID {id}: {ex.Message}");
                    return ResponseHelper.ResponseError<BalanceDto>(
                        CodesConstant.INTERNAL_SERVER_ERROR, "Error al crear el balance inicial."
                    );
                }
            }
        }

    }
}
