using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Dtos.Journal;
using ProyectoExamenU2.Dtos.Journal.DtosHelper;
using ProyectoExamenU2.Dtos.Logs;
using ProyectoExamenU2.Helpers;
using ProyectoExamenU2.Services.Interfaces;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProyectoExamenU2.Services
{
    public class JournalService : IJournalService
    {
        private readonly ProyectoExamenU2Context _context;
        private readonly ILoggerDBService _loggerDB;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;
        private readonly ILogger<JournalService> _logger;
        private readonly IBalanceService _balanceService;

        public JournalService(
            ProyectoExamenU2Context context,
            ILoggerDBService loggerDB,
            IMapper mapper,
            IAuditService auditService,
            ILogger<JournalService> logger,
            IBalanceService balanceService
            )
        {
            this._context = context;
            this._loggerDB = loggerDB;
            this._mapper = mapper;
            this._auditService = auditService;
            this._logger = logger;
            this._balanceService = balanceService;
        }
        public async Task<ResponseDto<JournalDto>> CreateJournalEntry(JournalEntryCreateDto dto)
        {
            Guid logId = Guid.Empty;
            // Guid userID = new Guid(_auditService.GetUserId());
            var userID = Guid.NewGuid();


            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.JOURNAL_ENTRY,
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


                    if (lastJournalEntryEntity != null)
                    {
                        if (dto.Date < lastJournalEntryEntity.Date)
                        {
                            await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}:> Fecha: {dto.Date}?? Ultimated Record date -> {lastJournalEntryEntity.Date}");
                            return ResponseHelper.ResponseError<JournalDto>(
                                CodesConstant.BAD_REQUEST,
                                $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> Fecha: {dto.Date}?? Ultimated Record date -> {lastJournalEntryEntity.Date}");
                        }
                    }
                    
                    if (DateTime.Compare(dto.Date.Date, DateTime.Now.Date) > 0)
                    {
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}:> Fecha: {dto.Date}?? {LogsMessagesConstant.FUTURE_DATE_ERROR}");
                        return ResponseHelper.ResponseError<JournalDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> Fecha: {dto.Date}??  ->{LogsMessagesConstant.FUTURE_DATE_ERROR} ");
                    }

                    // validacion que si existean las cuentas que me manda
                    // si los movimientos seran validos 
                    foreach (var accountEntry in dto.AccountsEntrys)
                    {
                        bool accountExists = await _context.AccountCatalogs.AnyAsync(a => a.Id == accountEntry.AccountId);
                        //validando si existe
                        if (!accountExists)
                        {
                            await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.NOT_FOUND}:> {accountEntry}?? {LogsMessagesConstant.NOT_FOUND}");
                            return ResponseHelper.ResponseError<JournalDto>(
                                CodesConstant.BAD_REQUEST,
                                $"{MessagesConstant.RECORD_NOT_FOUND} => { LogsMessagesConstant.NOT_FOUND}:> { accountEntry}?? { LogsMessagesConstant.NOT_FOUND}");
                        }
                        //Obteniendo la Cuenta
                        var accountEntity = await _context.AccountCatalogs.FindAsync(accountEntry.AccountId);


                        // validando estados de la cuenta
                        // que sena activos y acepten movimientos

                        // la cuenta permite movimientos pero no esta activa
                        var (error, status) = await ValidationsMovements(logId, accountEntity);
                        if (!status)
                        {
                            return error;
                        }

                        var accountType = accountEntity.BehaviorType;
                        // simulando la transaccion
                        var AccountBalance = await _context.Balances.FirstOrDefaultAsync(e => e.AccountCatalogId == accountEntry.AccountId);

                        // varificando que si exista el balance
                        // en teoria cada que se crea una cuenta se realiza esto pero si no es un error critico
                        if (AccountBalance == null)
                        {
                            throw new Exception($"{LogsMessagesConstant.API_ERROR}=> {LogsMessagesConstant.INVALID_DATA}: {MessagesConstant.RECORD_NOT_FOUND} :{accountEntry.AccountId}." +
                                $"{TablesConstant.BALANCES}");
                        }
                        // verificando o simulando el movimiento para saber si sera valido 
                        var ( errorSimulatedTransaccion, statusSimulatedTransaction) =  await SimulatedTransacction(logId, accountEntry, accountType, AccountBalance);
                        if (!statusSimulatedTransaction)
                        {
                            return errorSimulatedTransaccion;
                        }
                    }

                    // guardando la partida 

                    var EntryEntity = _mapper.Map<JournalEntryEntity>(dto);
                    // me daba error por alguna razon decia que por si era nullo 
                    // le metio eso dentro de corchetes por si acaso no existe 
                    var maxEntryNumber = _context.JournalEntries.Max(e => (int?)e.EntryNumber) ?? 0;
                    EntryEntity.EntryNumber = maxEntryNumber + 1;
                    logDetail.NewValues = JsonSerializer.Serialize(EntryEntity);
                    logDetail.EntityRowId = EntryEntity.Id;

                    _context.JournalEntries.Add(EntryEntity);
                    await _context.SaveChangesAsync();

                    // guardando y creando 2 un log por actualizacion
                    // log de la entrada 
                    // log del detalle creado de la partida
                    await CreateDetailsEntry(dto, userID, EntryEntity);
                    await transaction.CommitAsync();
                    var result = await _balanceService.UpdateAllBalancesAsync(userID);

                    if (!result) throw new InvalidOperationException("Error interno en la API al actualizar los saldos.");


                    var entity = _mapper.Map<JournalDto>(EntryEntity);

                    await _loggerDB.UpdateLogDetails(logDetail, logId, CodesConstant.CREATED, $"{LogsMessagesConstant.COMPLETED_SUCCESS}");

                    return ResponseHelper.ResponseSuccess<JournalDto>(CodesConstant.OK, MessagesConstant.CREATE_SUCCESS, entity);

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


        }

        private async Task<(ResponseDto<JournalDto>,bool)> SimulatedTransacction(Guid logId, AccountEntry accountEntry, char accountType, BalanceEntity AccountBalance)
        {
            if ((accountType == 'C' && accountEntry.muvType == 'D') || (accountType == 'D' && accountEntry.muvType == 'C'))
            {
                if ((AccountBalance.BalanceAmount - accountEntry.amount) < 0)
                {
                    await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                    $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.AMOUNT_ERROR} {accountEntry}");

                    return (ResponseHelper.ResponseError<JournalDto>(
                        CodesConstant.BAD_REQUEST,
                        $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.AMOUNT_ERROR} :{accountEntry} "
                    ),false);
                }
            }
            return (null, true);
        }

        private async Task<(ResponseDto<JournalDto>, bool)> ValidationsMovements(Guid logId, AccountCatalogEntity accountEntity)
        {
            if (accountEntity.AllowsMovement && !accountEntity.IsActive)
            {
                await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                    $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}");

                return (ResponseHelper.ResponseError<JournalDto>(
                    CodesConstant.BAD_REQUEST,
                    $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}"
                ), false);
            }
            // si la cuenta no esta activa pero permite movbimientos
            if (!accountEntity.AllowsMovement && accountEntity.IsActive)
            {
                await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                    $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.BEBIAROR_ERROR}");

                return (ResponseHelper.ResponseError<JournalDto>(
                    CodesConstant.BAD_REQUEST,
                    $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.BEBIAROR_ERROR}"
                ), false);
            }
            //la cuenta no esta activa y no permite movimiento 
            if (!accountEntity.AllowsMovement && !accountEntity.IsActive)
            {

                await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId,
                    $"{LogsMessagesConstant.INVALID_DATA}:> {LogsMessagesConstant.BEBIAROR_ERROR} && {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}");

                return (ResponseHelper.ResponseError<JournalDto>(
                    CodesConstant.BAD_REQUEST,
                    $"{MessagesConstant.CREATE_ERROR} => {LogsMessagesConstant.INVALID_DATA} :> {LogsMessagesConstant.BEBIAROR_ERROR} && {LogsMessagesConstant.ACTIVE_ACCOUNT_ERROR}"
                ),false);
            }
            return (null , true );
        }

        private async Task CreateDetailsEntry(JournalEntryCreateDto dto, Guid userID, JournalEntryEntity EntryEntity)
        {
            foreach (var accountEntry in dto.AccountsEntrys)
            {
                var accounEntity = await _context.AccountCatalogs.FindAsync(accountEntry.AccountId);
                var OldValueAccountEntry = await _context.Balances.FirstOrDefaultAsync(e => e.AccountCatalogId == accountEntry.AccountId);

                // detalle del log para cada entrada
                var logDetailBalance = new LogDetailDto
                {
                    Id = Guid.NewGuid(),
                    EntityTableName = TablesConstant.BALANCES,
                    EntityRowId = OldValueAccountEntry.Id,
                    ChangeType = MessagesConstant.UPDATE,
                    OldValues = JsonSerializer.Serialize(OldValueAccountEntry) ?? "",
                    NewValues = null,
                };


                //Mapeo del Detail para guardarlo en la base de datos 

                var detailEntryEntity = _mapper.Map<JournalEntryDetailEntity>(accountEntry);
                detailEntryEntity.JournalEntryId = EntryEntity.Id;
                detailEntryEntity.AccountCatalogId = accounEntity.Id;
                detailEntryEntity.EntryType = accountEntry.muvType;


                // detalle del log de las entry journal details 
                var logDetailEntry = new LogDetailDto
                {
                    Id = Guid.NewGuid(),
                    EntityTableName = TablesConstant.JOURNAL_ENTRY_DETAIL,
                    EntityRowId = null,
                    ChangeType = MessagesConstant.CREATE,
                    OldValues = null,
                    NewValues = JsonSerializer.Serialize(detailEntryEntity),
                };

                // log de la creacion del detalle 
                var logDeatilJornailEntrys = new LogCreateDto
                {
                    UserId = userID,
                    ActionType = AcctionsConstants.DATA_CREATED,
                    Status = CodesConstant.PENDING,
                    Message = $"{LogsMessagesConstant.PENDING}",
                    DetailId = logDetailBalance.Id,
                    ErrorId = null,
                };


                // validando que tipo de movimiento es 
                // movimiento igyual a la naturaleza de la cuenta
                 OldValueAccountEntry.BalanceAmount = AmountMovType(accountEntry, accounEntity, OldValueAccountEntry);
                //actualizando los datos para el guardado
                OldValueAccountEntry.Date = DateTime.Now;
                logDetailBalance.NewValues = JsonSerializer.Serialize(OldValueAccountEntry);
                //logDetailBalance.EntityRowId = OldValueAccountEntry.Id;
                logDetailEntry.EntityRowId = Guid.Empty;
                // Creando el log
                var logEntry = new LogCreateDto
                {
                    UserId = userID,
                    ActionType = AcctionsConstants.DATA_UPDATED,
                    Status = CodesConstant.PENDING,
                    Message = $"{LogsMessagesConstant.PENDING}",
                    DetailId = logDetailBalance.Id,
                    ErrorId = null,
                };

                // creando las instancias de los Logs
                var logEntrId = await _loggerDB.LogCreateLog(logDetailBalance, logEntry);
                var logEntryDetailId = await _loggerDB.LogCreateLog(logDetailEntry, logDeatilJornailEntrys);
                try
                {

                    _context.Balances.Update(OldValueAccountEntry);
                    _context.JournalEntryDetails.Add(detailEntryEntity);
                    await _context.SaveChangesAsync();
                    logDetailEntry.EntityRowId = detailEntryEntity.Id;
                    await _loggerDB.UpdateLogDetails(logDetailBalance, logEntrId, CodesConstant.OK, LogsMessagesConstant.UPDATE_SUCCESS);
                    await _loggerDB.UpdateLogDetails(logDetailEntry, logEntryDetailId, CodesConstant.OK, LogsMessagesConstant.COMPLETED_SUCCESS);

                }
                catch (Exception ex)
                {

                    var logError = new LogErrorCreateDto
                    {
                        ErrorCode = CodesConstant.INTERNAL_SERVER_ERROR.ToString(),
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace,
                        TargetSite = ex.TargetSite.ToString(),
                    };

                    await _loggerDB.LogError(logEntrId, CodesConstant.INTERNAL_SERVER_ERROR, logError, LogsMessagesConstant.API_ERROR);

                    throw;
                }

                //await _loggerDB.UpdateLogDetails();

            }
        }

        private static decimal AmountMovType(AccountEntry accountEntry, AccountCatalogEntity accounEntity, BalanceEntity OldValueAccountEntry)
        {
            if ((accounEntity.BehaviorType == 'C' && accountEntry.muvType == 'C') || (accounEntity.BehaviorType == 'D' && accountEntry.muvType == 'D'))
            {
                return  OldValueAccountEntry.BalanceAmount + accountEntry.amount;

            }
            // movimiento no natural al de la cuenta
            if ((accounEntity.BehaviorType == 'C' && accountEntry.muvType == 'D') || (accounEntity.BehaviorType == 'D' && accountEntry.muvType == 'C'))
            {
                return  OldValueAccountEntry.BalanceAmount - accountEntry.amount;

            }
            throw new Exception("Type Muvement is invalid");
        }

        public async Task<ResponseDto<PaginationDto<List<JournalDto>>>> GetProductsListAsync(searchJournalDto searchJournalDto)
        {
            Guid logId = Guid.Empty;
            // Guid userID = new Guid(_auditService.GetUserId());
            var userID = Guid.NewGuid();


            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.JOURNAL_ENTRY,
                EntityRowId = Guid.Empty,
                ChangeType = MessagesConstant.GET,
                OldValues = null,
                NewValues = JsonSerializer.Serialize(searchJournalDto)
            };

            //si no esta authenticado
            if (userID == Guid.Empty)
            {
                var logExeption = new LogCreateDto
                {
                    UserId = userID,
                    ActionType = AcctionsConstants.DATA_GET,
                    Status = CodesConstant.UNAUTHORIZED,
                    Message = $"{LogsMessagesConstant.NO_AUTHENTICATION}",
                    DetailId = logDetail.Id,
                    ErrorId = null,
                };

                logId = await _loggerDB.LogCreateLog(logDetail, logExeption);

                return ResponseHelper.ResponseError<PaginationDto<List<JournalDto>>>(
                      CodesConstant.BAD_REQUEST,
                      $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.UNAUTHENTICATED_USER_ERROR}");
            }

            // Creando el log
            var log = new LogCreateDto
            {
                UserId = userID,
                ActionType = AcctionsConstants.DATA_GET,
                Status = CodesConstant.PENDING,
                Message = $"{LogsMessagesConstant.PENDING}",
                DetailId = logDetail.Id,
                ErrorId = null,
            };


            logId = await _loggerDB.LogCreateLog(logDetail, log);

            try
            {

                var page = searchJournalDto.Page;
                var accountsId = searchJournalDto.Guids;
                var searchTerm = searchJournalDto.SearchTerm;


                const int PAGE_SIZE = 10;
                int startIndex = (page - 1) * PAGE_SIZE;

                // Consulta Base patra la Bd
                IQueryable<JournalEntryEntity> journalQuery = _context.JournalEntries
                    .Include(journal => journal.JournalEntryDetails) // incluye los detalles
                    .ThenInclude(ditail => ditail.Account)        // Incluye las ctas de los detalles
                    .AsQueryable();

                // Aplicando el Filtro por cuentas si existe 
                if (accountsId != null && accountsId.Any())
                {
                    // todas las partidas que contengan alguna de los id mandados de cuentas
                    //el journal  donde se tiene almenos accounts en la columna de cuentas de catalogo 
                    journalQuery = journalQuery.Where(journal => accountsId.All(id => journal.JournalEntryDetails.Any(d => d.AccountCatalogId == id)));
                }


                // busqueda 
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    journalQuery = journalQuery.Where(j =>
                        j.Description.ToLower().Contains(searchTerm.ToLower()) || j.JournalEntryDetails.Any(d => d.Account.AccountName.ToLower().Contains(searchTerm.ToLower())));
                }

                int totalItems = await journalQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalItems / PAGE_SIZE);
                var journals = await journalQuery
                    .OrderBy(j => j.Date)
                    .Skip(startIndex)
                    .Take(PAGE_SIZE)
                    .ToListAsync();

                // Mapeo a DTO
                var journalDtos = journals.Select(journal => new JournalDto
                {
                    Id = journal.Id,
                    EntryNumber = journal.EntryNumber,
                    Description = journal.Description,
                    Date = journal.Date,
                    JournalEntryDetails = journal.JournalEntryDetails.Select(detail => new JournalEntryDetailDto
                    {
                        Id = detail.Id,
                        JournalEntryId = detail.JournalEntryId,
                        AccountCatalogId = detail.AccountCatalogId,
                        Account = detail.Account,
                        Amount = detail.Amount,
                        EntryType = detail.EntryType
                    }).ToList()
                }).ToList();

                var result = new ResponseDto<PaginationDto<List<JournalDto>>>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Listado de partidas obtenido correctamente",
                    Data = new PaginationDto<List<JournalDto>>
                    {
                        CurrentPage = page,
                        PageSize = PAGE_SIZE,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = journalDtos,
                        HasPreviousPage = page > 1,
                        HasNextPage = page < totalPages
                    }
                };
                logDetail.NewValues = JsonSerializer.Serialize(result);
                await _loggerDB.UpdateLogDetails(logDetail, logId, CodesConstant.OK, LogsMessagesConstant.COMPLETED_SUCCESS);
                // respuesta
                return result;
            }catch(Exception ex)
            {
                var logError = new LogErrorCreateDto
                {
                    ErrorCode = CodesConstant.INTERNAL_SERVER_ERROR.ToString(),
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    TargetSite = ex.TargetSite.ToString(),
                };

                await _loggerDB.LogError(logId, CodesConstant.INTERNAL_SERVER_ERROR, logError, LogsMessagesConstant.API_ERROR);

                _logger.LogError(ex, "Error al Invocar la lista de Partidas");
                return ResponseHelper.ResponseError<PaginationDto<List<JournalDto>>>(
                    CodesConstant.INTERNAL_SERVER_ERROR,
                    $"{MessagesConstant.UPDATE_ERROR} => {MessagesConstant.API_FATAL_ERROR} :: {ex.Message} {ex.TargetSite} ");
            }
        }

        public async Task<ResponseDto<List<JournalDto>>> GetJournalTop5ListAsync(int topQuantity = 5 )
        {
            Guid logId = Guid.Empty;
            var userID = Guid.NewGuid(); 

            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.JOURNAL_ENTRY,
                EntityRowId = Guid.Empty,
                ChangeType = MessagesConstant.GET,
                OldValues = null,
                NewValues = null,
            };

            if (userID == Guid.Empty)
            {
                var logExeption = new LogCreateDto
                {
                    UserId = userID,
                    ActionType = AcctionsConstants.DATA_GET,
                    Status = CodesConstant.UNAUTHORIZED,
                    Message = $"{LogsMessagesConstant.NO_AUTHENTICATION}",
                    DetailId = logDetail.Id,
                    ErrorId = null,
                };

                logId = await _loggerDB.LogCreateLog(logDetail, logExeption);

                return ResponseHelper.ResponseError<List<JournalDto>>(
                    CodesConstant.UNAUTHORIZED,
                    $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.UNAUTHENTICATED_USER_ERROR}");
            }

            var log = new LogCreateDto
            {
                UserId = userID,
                ActionType = AcctionsConstants.DATA_GET,
                Status = CodesConstant.PENDING,
                Message = $"{LogsMessagesConstant.PENDING}",
                DetailId = logDetail.Id,
                ErrorId = null,
            };

            logId = await _loggerDB.LogCreateLog(logDetail, log);

            try
            {
                // Consulta los 5 diarios mas recientes porla fecha
                var journals = await _context.JournalEntries
                    .Include(journal => journal.JournalEntryDetails)
                        .ThenInclude(detail => detail.Account)
                    .OrderByDescending(j => j.EntryNumber) 
                    .Take(topQuantity)
                    .ToListAsync();
                // el to take si necesitasmas de 5 es aqui 
                


                // mapeo al dto de respuesta
                var journalDtos = journals.Select(journal => new JournalDto
                {
                    Id = journal.Id,
                    EntryNumber = journal.EntryNumber,
                    Description = journal.Description,
                    Date = journal.Date,
                    JournalEntryDetails = journal.JournalEntryDetails.Select(detail => new JournalEntryDetailDto
                    {
                        Id = detail.Id,
                        JournalEntryId = detail.JournalEntryId,
                        AccountCatalogId = detail.AccountCatalogId,
                        Account = detail.Account,
                        Amount = detail.Amount,
                        EntryType = detail.EntryType
                    }).ToList()
                }).ToList();

                var result = new ResponseDto<List<JournalDto>>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = $"{MessagesConstant.RECORDS_FOUND}",
                    Data = journalDtos
                };

                logDetail.NewValues = JsonSerializer.Serialize(result);
                await _loggerDB.UpdateLogDetails(logDetail, logId, CodesConstant.OK, LogsMessagesConstant.COMPLETED_SUCCESS);

                return result;
            }
            catch (Exception ex)
            {
                var logError = new LogErrorCreateDto
                {
                    ErrorCode = CodesConstant.INTERNAL_SERVER_ERROR.ToString(),
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    TargetSite = ex.TargetSite.ToString(),
                };

                await _loggerDB.LogError(logId, CodesConstant.INTERNAL_SERVER_ERROR, logError, LogsMessagesConstant.API_ERROR);

                _logger.LogError(ex, $"Error al obtener los ultimos registros de partidas  {MessagesConstant.API_FATAL_ERROR}");
                return ResponseHelper.ResponseError<List<JournalDto>>(
                    CodesConstant.INTERNAL_SERVER_ERROR,
                    $"{MessagesConstant.UPDATE_ERROR} => {MessagesConstant.API_FATAL_ERROR} :: {ex.Message} {ex.TargetSite}");
            }
        }

    }


}
