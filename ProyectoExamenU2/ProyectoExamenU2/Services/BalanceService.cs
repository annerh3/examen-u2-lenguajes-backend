using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
        // TODO AGREGAR LOS LOGS DE LOS CAMBIOS--- QUE VA REALIZANDO LISTO
        // YA DENME EL TITULO CON ESTO TREBNDO SOY BARBARO TITO 
        // Pale estaria orgulloso de mi 
        // TODO PROBAR YA EN EL FRONTEND CONNECTANDO 
        public async Task<decimal> ActualizarSaldoYCrearLogsRecursivoAsync(AccountCatalogEntity cuenta, Guid userId)
        {
            decimal nuevoSaldo = 0;

            // Esta es la parte recursiva donde si se tienen cuentas hijas se activanuevamente 
            if (cuenta.ChildAccounts.Any())
            {
                //si tiene hijas para cada hija haz lo siguiente 
                foreach (var hija in cuenta.ChildAccounts)
                {
                    var cuentaHijaConHijas = await _context.AccountCatalogs
                        .Include(c => c.ChildAccounts)
                        .FirstOrDefaultAsync(c => c.Id == hija.Id);

                    // llamada recursiva que termina retornando el nuevo saldo 
                    nuevoSaldo += await ActualizarSaldoYCrearLogsRecursivoAsync(cuentaHijaConHijas, userId);
                }
            }
            // si no tiene mas hijas o en una rama llego al fdinal de ella
            // ejemplo 
            // Activos
            //  - Activos Corrientes
            //    - Efectivo
            else
            {
                // como no tiene hijas retorna el saldo directamente 
                var balance = await _context.Balances.FirstOrDefaultAsync(b => b.AccountCatalogId == cuenta.Id);
                nuevoSaldo = balance?.BalanceAmount ??  0;
                

            }
            // Crear el log para la cuenta actual
            // obteniendo el valor anterior 
            var saldoAnterior = await _context.Balances
                .Where(b => b.AccountCatalogId == cuenta.Id)
                .Select(b => b.BalanceAmount)
                .FirstOrDefaultAsync();

            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.BALANCES,
                EntityRowId = cuenta.Id,
                ChangeType = MessagesConstant.UPDATE,
                OldValues = JsonSerializer.Serialize(new { BalanceAmount = saldoAnterior }),
                NewValues = JsonSerializer.Serialize(new { BalanceAmount = nuevoSaldo })
            };

            var log = new LogCreateDto
            {
                UserId = userId,
                ActionType = AcctionsConstants.DATA_UPDATED,
                Status = CodesConstant.PENDING,
                Message = LogsMessagesConstant.PENDING,
                DetailId = logDetail.Id,
                ErrorId = null
            };

            var logId = await _loggerDB.LogCreateLog(logDetail, log);

            var balanceActual = await _context.Balances.FirstOrDefaultAsync(b => b.AccountCatalogId == cuenta.Id);

            // si viene vacio est0 es un error 
            // por que al momento de crearse se estan registrando
            if (balanceActual != null)
            {
                balanceActual.BalanceAmount = nuevoSaldo;
            }
            else
            {
                throw new Exception($"FATAL !!{LogsMessagesConstant.INVALID_DATA  }  =>> {LogsMessagesConstant.API_ERROR} ");
            }

            _context.Update(balanceActual);
            await _context.SaveChangesAsync();
            await _loggerDB.UpdateLogDetails(logDetail, logId, CodesConstant.OK, LogsMessagesConstant.COMPLETED_SUCCESS);

            // retorno al padre el saldo nuevo
            return nuevoSaldo;
        }

        public async Task<bool> UpdateAllBalancesAsync(Guid userId)
        {
            try
            {
                // Obtener todas las cuentas principales
                // Estan son las raiz 
                // cuentas con pre code null y cuenta parent id null
                var cuentasPadres = await _context.AccountCatalogs
                    .Where(c => c.ParentId == null)
                    .Include(c => c.ChildAccounts)
                    .ToListAsync();

                foreach (var cuentaPadre in cuentasPadres)
                {
                    //esta funcion recibe el primer hijo 
                    // calcula el saldo recursivamente para cada hija dentro de el 
                    await ActualizarSaldoYCrearLogsRecursivoAsync(cuentaPadre, userId);
                }

                return true;
            }
            catch (Exception e)
            {
                throw new Exception($"{LogsMessagesConstant.API_ERROR} => {LogsMessagesConstant.INVALID_DATA} ERROR EN LA RECURCION ::{e}");
            }
        }


    }
}
