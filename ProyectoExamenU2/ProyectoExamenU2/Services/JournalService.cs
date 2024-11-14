using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Dtos.Journal;
using ProyectoExamenU2.Dtos.Logs;
using ProyectoExamenU2.Helpers;
using ProyectoExamenU2.Services.Interfaces;
using System.Text.Json;

namespace ProyectoExamenU2.Services
{
    public class JournalService : IJournalService
    {
        private readonly ProyectoExamenU2Context _context;
        private readonly ILoggerDBService _loggerDB;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;
        private readonly ILogger<JournalService> _logger;

        public JournalService(
            ProyectoExamenU2Context context,
            ILoggerDBService loggerDB,
            IMapper mapper,
            IAuditService auditService,
            ILogger<JournalService> logger
            )
        {
            this._context = context;
            this._loggerDB = loggerDB;
            this._mapper = mapper;
            this._auditService = auditService;
            this._logger = logger;
        }
        public async Task<ResponseDto<JournalDto>> CreateEvent(JournalEntryCreateDto dto)
        {
            Guid logId = Guid.Empty;
            Guid userID = new Guid(_auditService.GetUserId());


            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.BALANCES,
                EntityRowId = Guid.Empty,
                ChangeType = MessagesConstant.CREATE,
                OldValues = null,
                NewValues = JsonSerializer.Serialize(dto)
            };

            //si no esta authenticado
            if (userID == Guid.Empty)
            {
                var logExeption = new LogCreateDto
                {
                    UserId = userID,
                    ActionType = AcctionsConstants.DATA_CREATED,
                    Status = CodesConstant.UNAUTHORIZED,
                    Message = $"{LogsMessagesConstant.NO_AUTHENTICATION}",
                    DetailId = logDetail.Id,
                    ErrorId = null,
                };

                logId = await _loggerDB.LogCreateLog(logDetail, logExeption);

                return ResponseHelper.ResponseError<JournalDto>(
                        CodesConstant.BAD_REQUEST,
                        $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.UNAUTHENTICATED_USER_ERROR}");
            }
            
            // Creando el log
            var log = new LogCreateDto
            {
                UserId = userID,
                ActionType = AcctionsConstants.DATA_CREATED,
                Status = CodesConstant.PENDING,
                Message = $"{LogsMessagesConstant.PENDING}",
                DetailId = logDetail.Id,
                ErrorId = null,
            };


            logId = await _loggerDB.LogCreateLog(logDetail, log);

            using (var transaction = await _context.Database.BeginTransactionAsync()) 
            {
                try
                {
                    //  Descrion no vacia
                    if (string.IsNullOrWhiteSpace(dto.Description))
                    {
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}");
                        return ResponseHelper.ResponseError<JournalDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> Description: {dto.Description}??");
                    }

                    // fecha existe 
                    if (dto.Date == default(DateTime))
                    {
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}");
                        return ResponseHelper.ResponseError<JournalDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> Fecha: {dto.Date}??");
                    }

                    //validacion de la fecha mandada no sea menor a la ultima registrada
                    var lastJournalEntryEntity = await _context.JournalEntries
                                .OrderByDescending(j => j.Date)
                                .FirstOrDefaultAsync();

                    if (dto.Date < lastJournalEntryEntity.Date)
                    {
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}:> Fecha: {dto.Date}?? Ultimated Record date -> {lastJournalEntryEntity.Date}");
                        return ResponseHelper.ResponseError<JournalDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> Fecha: {dto.Date}?? Ultimated Record date -> {lastJournalEntryEntity.Date}");
                    }

                    if (dto.Date > DateTime.Now)
                    {
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}:> Fecha: {dto.Date}?? {LogsMessagesConstant.FUTURE_DATE_ERROR}");
                        return ResponseHelper.ResponseError<JournalDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> Fecha: {dto.Date}??  ->{LogsMessagesConstant.FUTURE_DATE_ERROR} ");
                    }

                    // validacion que si existean las cuentas que me manda
                    foreach (var accountEntry in dto.AccountsEntrys)
                    {
                        bool accountExists = await _context.AccountCatalogs.AnyAsync(a => a.Id == accountEntry.AccountId);
                        //validando si existe
                        if (!accountExists)
                        {
                            await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}:> Fecha: {dto.Date}?? {LogsMessagesConstant.FUTURE_DATE_ERROR}");
                            return ResponseHelper.ResponseError<JournalDto>(
                                CodesConstant.BAD_REQUEST,
                                $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> Fecha: {dto.Date}??  ->{LogsMessagesConstant.FUTURE_DATE_ERROR} ");
                        }
                        //Obteniendo la Cuenta
                        var accountEntity = await _context.AccountCatalogs.FindAsync(accountEntry.AccountId);
                        // validando estados de la cuenta

                        // TODO HACER ESTO UN SWIC MEJOR 
                        // -----------------------------------------------------------
                        // la cuenta permite movimientos pero no esta activa
                        if (accountEntity.AllowsMovement && !accountEntity.IsActive)
                        {
                            await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                                $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}");

                            return ResponseHelper.ResponseError<JournalDto>(
                                CodesConstant.BAD_REQUEST,
                                $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}"
                            );
                        }
                        // si la cuenta no esta activa pero permite movbimientos
                        if (!accountEntity.AllowsMovement && accountEntity.IsActive)
                        {
                            await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                                $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.BEBIAROR_ERROR}");

                            return ResponseHelper.ResponseError<JournalDto>(
                                CodesConstant.BAD_REQUEST,
                                $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.BEBIAROR_ERROR}"
                            );
                        }
                        //la cuenta no esta activa y no permite movimiento 
                        if (!accountEntity.AllowsMovement && !accountEntity.IsActive)
                        {
                            
                            await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                                $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.BEBIAROR_ERROR} && {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}");

                            return ResponseHelper.ResponseError<JournalDto>(
                                CodesConstant.BAD_REQUEST,
                                $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.BEBIAROR_ERROR} && {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}"
                            );
                        }
                        // fin del sitch 
                        //-------------------------------------------------------------

                        var accountType = accountEntity.BehaviorType;
                        // simulando la transaccion
                        var AccountBalance = await _context.Balances.FindAsync(accountEntry.AccountId);

                        // varificando que si exista el balance
                        // en teoria cada que se crea una cuenta se realiza esto pero si no es un error critico
                        if (AccountBalance == null)
                        {
                            throw new Exception($"{LogsMessagesConstant.API_ERROR}=> {LogsMessagesConstant.INVALID_DATA}: {MessagesConstant.RECORD_NOT_FOUND} :{accountEntry.AccountId}." +
                                $"{TablesConstant.BALANCES}");
                        }

                        if(accountType == 'C'  && accountEntry.muvType == 'D')
                        {
                            if((AccountBalance.BalanceAmount - accountEntry.amount) < 0)
                            {
                                await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                                $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.AMOUNT_ERROR} {accountEntry}");

                                return ResponseHelper.ResponseError<JournalDto>(
                                    CodesConstant.BAD_REQUEST,
                                    $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.AMOUNT_ERROR} :{accountEntry} "
                                );
                            } 
                        }
                        if (accountType == 'D' && accountEntry.muvType == 'C')
                        {
                            if ((AccountBalance.BalanceAmount - accountEntry.amount) < 0)
                            {
                                await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                                $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.AMOUNT_ERROR} {accountEntry}");

                                return ResponseHelper.ResponseError<JournalDto>(
                                    CodesConstant.BAD_REQUEST,
                                    $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.AMOUNT_ERROR} :{accountEntry} "
                                );
                            }
                        }




                    }

                }
                catch (Exception ex) 
                {
                    await transaction.RollbackAsync();
                    var logError = new LogErrorCreateDto
                    {
                        ErrorCode = CodesConstant.INTERNAL_SERVER_ERROR.ToString(),
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace,
                        TargetSite = ex.TargetSite.ToString(),
                    };

                    await _loggerDB.LogError(logId, CodesConstant.INTERNAL_SERVER_ERROR, logError, LogsMessagesConstant.API_ERROR);

                    _logger.LogError(ex, "Error al Crear Una Partida en el try.");
                    return ResponseHelper.ResponseError<JournalDto>(
                        CodesConstant.INTERNAL_SERVER_ERROR,
                        $"{MessagesConstant.UPDATE_ERROR} => {MessagesConstant.API_FATAL_ERROR} :: {ex.Message} {ex.TargetSite} ");
                }

            }



            throw new NotImplementedException();
        }
    }
}
