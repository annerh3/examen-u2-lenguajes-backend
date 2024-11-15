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
using System.Text.Json;

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
            Guid idPrueba = Guid.NewGuid();
            Guid logId = Guid.Empty;
            var status = 0;
            var message = "";
            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),  
                EntityTableName = TablesConstant.BALANCES,
                EntityRowId = Guid.Empty,
                ChangeType = MessagesConstant.CREATE,
                OldValues = null,
                NewValues = JsonSerializer.Serialize(new { AccountCatalogId = id, BalanceAmount = 0, Date = DateTime.UtcNow })
            };

            // Creando el log
            var log = new LogCreateDto
            {
                UserId = idPrueba,  
                ActionType = AcctionsConstants.DATA_CREATED,
                Status = CodesConstant.PENDING,
                Message = $"{LogsMessagesConstant.PENDING}",
                DetailId = logDetail.Id,  
                ErrorId = null,
            };


            logId = await _loggerDB.LogCreateLog(logDetail, log);
            //await _loggerDB.LogCreateActionAsync(log);
            try
            {
                // Guardar el log inicial 
               
                // transaccion
                using (var transaction = await _context.Database.BeginTransactionAsync())
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

                        // Actualizacion de los elementos de Log 
                        logDetail.EntityRowId = newBalance.Id;
                        status = CodesConstant.CREATED;
                        message = $"{LogsMessagesConstant.COMPLETED_SUCCESS}";

                        await _loggerDB.UpdateLogDetails(logDetail,logId,status , message);
                        await transaction.CommitAsync();

                        var balanceDto = _mapper.Map<BalanceDto>(newBalance);
                        return ResponseHelper.ResponseSuccess(
                            CodesConstant.CREATED, "Balance inicial creado exitosamente.", balanceDto
                        );
                    }

                    var entityBalance = _mapper.Map<BalanceDto>(existingBalance);
                    status = CodesConstant.CONFLICT;
                    message = $"{LogsMessagesConstant.RECORD_ALREADY_EXISTS}";

                    await _loggerDB.UpdateLogDetails(logDetail, logId, status, message);

                    return ResponseHelper.ResponseSuccess(
                        CodesConstant.OK, "El balance ya existe", entityBalance
                    );
                }
            }
            catch (Exception ex)
            {
                // Actualizando el LOG MEDIANTE LOS Errores , agregando tambien la StacTrace
                status = CodesConstant.INTERNAL_SERVER_ERROR;
                message = $"{LogsMessagesConstant.API_ERROR}";
                var logError = new LogErrorCreateDto
                {
                    ErrorCode = CodesConstant.INTERNAL_SERVER_ERROR.ToString(),
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                };

                await _loggerDB.LogError(logId, status, logError, message);
                _logger.LogError($"Error al crear el balance inicial para el ID {id}: {ex.Message}");

                return ResponseHelper.ResponseError<BalanceDto>(
                    CodesConstant.INTERNAL_SERVER_ERROR, "Error al crear el balance inicial."
                );
            }
        }

        // funcion recursiva
        // TODO AGREGAR LOS LOGS DE LOS CAMBIOS QUE VA REALIZANDO 
        public async Task<decimal> CalcularSaldoAsync(AccountCatalogEntity account)
        {




            var balance = await _context.Balances.FirstOrDefaultAsync(b => b.AccountCatalogId == account.Id);

            // si la cuenta no tiene hijas
            // el saldo se retorna
            // si es que hay si no encuentra osea queda nullo manda 0 
            if (account.ChildAccounts.Count == 0)
            {
                return balance?.BalanceAmount ?? 0; 
            }

           
            decimal saldoHijas = 0;
            foreach (var hija in account.ChildAccounts)
            {
                // recuersion llamado a las hijas de la cuenta 
                var hijaConHijas = await _context.AccountCatalogs
                    .Include(a => a.ChildAccounts)
                    .FirstOrDefaultAsync(a => a.Id == hija.Id);

                saldoHijas += await CalcularSaldoAsync(hijaConHijas);
            }

            return saldoHijas;
        }
        public async Task<bool> UpdateAllBalancesAsync()
        {
            try
            {
                var cuentas = await _context.AccountCatalogs
                    .Where(c => c.ParentId == null)
                    .Include(c => c.ChildAccounts).ToListAsync();

                foreach (var cuenta in cuentas)
                {




                    var cuentaConHijas = await _context.AccountCatalogs
                        .Include(c => c.ChildAccounts)
                        .FirstOrDefaultAsync(c => c.Id == cuenta.Id);

                    // saldo
                    var nuevoSaldo = await CalcularSaldoAsync(cuentaConHijas);

                    // balance actual actualizado
                    var balance = await _context.Balances.FirstOrDefaultAsync(b => b.AccountCatalogId == cuenta.Id);
                    if (balance != null)
                    {
                        balance.BalanceAmount = nuevoSaldo;
                    }
                    else
                    {
                        // Si no existe creamos un nuevo balance
                        // se supone nunca deveria de pasar 
                        // creo que seria un error mas bien
                        balance = new BalanceEntity
                        {
                            Id = cuenta.Id,
                            BalanceAmount = nuevoSaldo
                        };
                        await _context.Balances.AddAsync(balance);
                    }
                    _context.Update(balance);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }



    }
}
