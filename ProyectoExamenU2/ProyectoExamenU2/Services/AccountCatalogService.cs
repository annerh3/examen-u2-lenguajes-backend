using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using System.Reflection;
using System.Text.Json;

namespace ProyectoExamenU2.Services
{
    public class AccountCatalogService : IAccountCatalogService
    {
        private readonly ProyectoExamenU2Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<IAccountCatalogService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBalanceService _balanceService;
        private readonly ILoggerDBService _loggerDB;
        private readonly IJournalService _journalService;

        public AccountCatalogService(
            ProyectoExamenU2Context context,
            IMapper mapper,
            ILogger<IAccountCatalogService> logger,
            IConfiguration configuration,
            IBalanceService balanceService,
            ILoggerDBService loggerDB,
            IJournalService journalService
            )
        {
            this._context = context;
            this._mapper = mapper;
            this._logger = logger ;
            this._configuration = configuration;
            this._balanceService = balanceService;
            this._loggerDB = loggerDB;
            this._journalService = journalService;
        }
        public async Task<ResponseDto<AccountDto>> CreateAcoountAsync(AccountCreateDto dto)
        {
            Guid idPrueba = Guid.NewGuid();
            Guid logId = Guid.Empty;
            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.ACCOUNT_CATALOG,
                EntityRowId = Guid.Empty,
                ChangeType = MessagesConstant.CREATE,
                OldValues = null,
                NewValues = JsonSerializer.Serialize(dto)
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



            try
            {
                // Mapeo de la entidad
                var accountEntity = _mapper.Map<AccountCatalogEntity>(dto);
                accountEntity.IsActive = true;

                // Validar el tipo de dato (D -> DEBIT o C -> CREDIT)
                if (dto.BehaviorType.ToString().ToUpper() != "D" && dto.BehaviorType.ToString().ToUpper() != "C")
                {
                    //Mandar Log -- datos invalidos
                    await _loggerDB.LogStateUpdate( CodesConstant.BAD_REQUEST , logId , $"{LogsMessagesConstant.INVALID_DATA}");

                    return ResponseHelper.ResponseError<AccountDto>(
                        CodesConstant.BAD_REQUEST,
                        $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.BehaviorTypeError}");
                }
                // Validar que no exista una cuenta con el mismo código completo
                var existingAccount = await _context.AccountCatalogs
                    .FirstOrDefaultAsync(account => (account.PreCode == dto.PreCode) && (account.Code == dto.Code));

                if (existingAccount != null)
                {
                    //Mandar Log -- Record already exist
                    await _loggerDB.LogStateUpdate(CodesConstant.CONFLICT, logId, $"{LogsMessagesConstant.RECORD_ALREADY_EXISTS}");
                    return ResponseHelper.ResponseError<AccountDto>(
                        CodesConstant.CONFLICT,
                        $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.AccountAlreadyExists} : {existingAccount.AccountName}");
                }
                // Validar si existe la cuenta padre
                var parentAccount = dto.ParentId.HasValue
                    ? await _context.AccountCatalogs.FirstOrDefaultAsync(a => a.Id == dto.ParentId.Value)
                    : null;

                if (accountEntity.ParentId.HasValue)
                {
                    if (parentAccount == null)
                    {
                        //Mandar Log
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}");
                        return ResponseHelper.ResponseError<AccountDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.RECORD_NOT_FOUND} : {dto.ParentId}");
                    }
                    string parentCompleteCode = $"{parentAccount.PreCode}{parentAccount.Code}";
                    if (parentCompleteCode != dto.PreCode)
                    {
                        //Mandar Log
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}");
                        return ResponseHelper.ResponseError<AccountDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.PreCodeMismatch}: Parent = {parentCompleteCode} , Account-PreCode {dto.PreCode}");
                    }


                   


                    /// Log para las cuentas actualizadas
                    var logDetailParentChild = new LogDetailDto
                    {
                        Id = Guid.NewGuid(),
                        EntityTableName = TablesConstant.ACCOUNT_CATALOG,
                        EntityRowId = parentAccount.Id,
                        ChangeType = MessagesConstant.UPDATE,
                        OldValues = JsonSerializer.Serialize(new
                        {
                            AllowsMovement = parentAccount.AllowsMovement,
                            ChildAccounts = parentAccount.ChildAccounts.Select(c => c.Id)
                        }),
                        NewValues = null,
                    };
                    // Log adicional para el cambio de padrehijo 
                    var logParentChild = new LogCreateDto
                    {

                        UserId = idPrueba,
                        ActionType = AcctionsConstants.DATA_UPDATED,
                        Status = CodesConstant.OK,
                        Message = $"{LogsMessagesConstant.UPDATE_SUCCESS}",
                        DetailId = logDetailParentChild.Id,
                        ErrorId = null,
                    };

                    // Asegurarse de que la cuenta padre tiene una lista de cuentas hijas
                    if (parentAccount.ChildAccounts == null)
                    {
                        // veriicando que la cuenta padre no tenga saldos
                        var amount = await _context.Balances.FindAsync(parentAccount.Id);
                        // si tiene saldo se hace una contrapartida
                        if (amount.BalanceAmount != 0)
                        {
                            var contraMuvTipeFather = (parentAccount.BehaviorType == 'C') ? 'D' : 'C';
                            var contraMuvChild = accountEntity.BehaviorType == 'C' ? 'D' : 'C';

                            var journalCreate = new JournalEntryCreateDto
                            {
                                Description = "Ajuste de los Saldos para la nueva subcuenta",
                                Date = DateTime.Now,
                                AccountsEntrys = [

                                    new AccountEntry{
                                        AccountId = parentAccount.Id,
                                        amount = amount.BalanceAmount,
                                        muvType = contraMuvTipeFather
                                    },
                                    new AccountEntry{
                                        AccountId = accountEntity.Id,
                                        amount = amount.BalanceAmount,
                                        muvType = contraMuvChild
                                    }
                                ]
                            };


                            await _journalService.CreateJournalEntry(journalCreate);

                        }

                        parentAccount.ChildAccounts = new List<AccountCatalogEntity>();
                    }
                    // Añadir la nueva cuenta hija y actualizar el estado de movimiento del padre
                    parentAccount.ChildAccounts.Add(accountEntity);
                    parentAccount.AllowsMovement = false;
                    _context.AccountCatalogs.Update(parentAccount);

                    logDetailParentChild.NewValues = JsonSerializer.Serialize(new
                    {
                        childs = parentAccount.ChildAccounts.Select(c => c.Id),
                    });
                    await _loggerDB.LogCreateLog(logDetailParentChild, logParentChild);

                }
                if((parentAccount == null && accountEntity.PreCode != null) )
                {
                    //|| accountEntity.PreCode  != ""
                    //Mandar Log
                    await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA} -> Father ??? and Pre code?? ");
                    return ResponseHelper.ResponseError<AccountDto>(
                        CodesConstant.BAD_REQUEST,
                        $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.PreCodeMismatch}: Parent = {null} ?? , Account-PreCode {dto.PreCode} ??");
                }


                // Proceder a crear la cuenta
                _context.AccountCatalogs.Add(accountEntity);
                await _context.SaveChangesAsync();


                logDetail.EntityRowId = accountEntity.Id;
                await _loggerDB.UpdateLogDetails(logDetail, logId, CodesConstant.CREATED, LogsMessagesConstant.COMPLETED_SUCCESS);

                // Crear balance inicial
                var result = await _balanceService.CreateInitBalance(accountEntity.Id);
                var accountResult = _mapper.Map<AccountDto>(accountEntity);

                // Respuesta de éxito
                return ResponseHelper.ResponseSuccess<AccountDto>(
                    CodesConstant.OK,
                    $"{MessagesConstant.CREATE_SUCCESS} => Record: {accountResult.Id} :: Name :{accountResult.AccountName} ",
                    accountResult);
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

                // Manejo de errores generales
                return ResponseHelper.ResponseError<AccountDto>(
                    CodesConstant.INTERNAL_SERVER_ERROR,
                    $"{MessagesConstant.CREATE_ERROR} => Error inesperado: {ex.Message}");
            }

        }

        public Task<ResponseDto<AccountDto>> DowAccountByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto<AccountDto>> EditAccountByIdAsync(AccountEditDto dto, Guid id)
        {
            Guid idPrueba = Guid.NewGuid();
            Guid logId = Guid.Empty;
            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.ACCOUNT_CATALOG,
                EntityRowId = Guid.Empty,
                ChangeType = MessagesConstant.UPDATE,
                OldValues = null,
                NewValues = JsonSerializer.Serialize(dto)
            };

            // Creando el log
            var log = new LogCreateDto
            {
                UserId = idPrueba,
                ActionType = AcctionsConstants.DATA_UPDATED,
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
                    // Busca la entidad en la base de datos
                    var existingAccount = await _context.AccountCatalogs.FirstOrDefaultAsync(a => a.Id == id);
                    // si no la encuentra
                    if (existingAccount is null)
                    {
                        //Mandar Log -- datos invalidos
                        await _loggerDB.LogStateUpdate(CodesConstant.NOT_FOUND, logId, $"{LogsMessagesConstant.NOT_FOUND}");

                        return ResponseHelper.ResponseError<AccountDto>(
                            CodesConstant.NOT_FOUND,
                            $"{MessagesConstant.UPDATE_ERROR} => {MessagesConstant.RECORD_NOT_FOUND} : {id}");
                    }


                    // Validacion del Tipo de Movimeinto
                    if (dto.BehaviorType.ToString().ToUpper() != "D" && dto.BehaviorType.ToString().ToUpper() != "C")
                    {
                        //Mandar Log -- datos invalidos
                        await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}");
                        return ResponseHelper.ResponseError<AccountDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.BehaviorTypeError}");
                    }
                           
                    // Logica para los padres
                    if (existingAccount.ParentId != dto.ParentId)
                    {
                            await _loggerDB.LogStateUpdate(CodesConstant.BAD_REQUEST, logId, $"{LogsMessagesConstant.INVALID_DATA}");
                            return ResponseHelper.ResponseError<AccountDto>(
                                CodesConstant.BAD_REQUEST,
                                $"{MessagesConstant.UPDATE_ERROR} : {MessagesConstant.PreCodeMismatch}?? -> Father Account Invalid Movement");
                    }

                    // Guarda los cambios en la base de datos
                    _mapper.Map(dto, existingAccount);
                    _context.Update(existingAccount);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    logDetail.EntityRowId = id;
                    await _loggerDB.UpdateLogDetails(logDetail, logId, CodesConstant.CREATED, LogsMessagesConstant.COMPLETED_SUCCESS);


                    // Retorna el DTO 
                    var result = _mapper.Map<AccountDto>(existingAccount);
                    return ResponseHelper.ResponseSuccess<AccountDto>(
                        CodesConstant.OK,
                        $"{MessagesConstant.UPDATE_SUCCESS} => Record : {id}", result);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    var logError = new LogErrorCreateDto
                    {
                        ErrorCode = CodesConstant.INTERNAL_SERVER_ERROR.ToString(),
                        ErrorMessage = e.Message,
                        StackTrace = e.StackTrace,
                        TargetSite =  e.TargetSite.ToString(),
                    };

                    await _loggerDB.LogError(logId, CodesConstant.INTERNAL_SERVER_ERROR, logError, LogsMessagesConstant.API_ERROR);

                    _logger.LogError(e, "Error al editar la cuenta en el try.");
                    return ResponseHelper.ResponseError<AccountDto>(
                        CodesConstant.INTERNAL_SERVER_ERROR,
                        $"{MessagesConstant.UPDATE_ERROR} => {MessagesConstant.API_FATAL_ERROR} : Record {id}");
                }
            }
        }

        public async Task<ResponseDto<AccountDto>> GetAccountByIdAsync(Guid id)
        {
            Guid idPrueba = Guid.NewGuid();
            Guid logId = Guid.Empty;
            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.ACCOUNT_CATALOG,
                EntityRowId = id,
                ChangeType = MessagesConstant.GET,
                OldValues = null,
                NewValues = null
            };

            // Creando el log
            var log = new LogCreateDto
            {
                UserId = idPrueba,
                ActionType = AcctionsConstants.DATA_GET,
                Status = CodesConstant.PENDING,
                Message = $"{LogsMessagesConstant.PENDING}",
                DetailId = logDetail.Id,
                ErrorId = null,
            };

            var AccountEntity = await _context.AccountCatalogs
                    .Include(account => account.ChildAccounts) // Incluye las cuentas hijas de primer nivel
                    .FirstOrDefaultAsync(ev => ev.Id == id);


            if (AccountEntity == null)
            {
                await _loggerDB.LogStateUpdate(CodesConstant.NOT_FOUND, logId, $"{LogsMessagesConstant.NOT_FOUND}");
                return ResponseHelper.ResponseError<AccountDto>(CodesConstant.NOT_FOUND, $"{MessagesConstant.RECORD_NOT_FOUND} : Record {id}");
            }
            var accountDto = _mapper.Map<AccountDto>(AccountEntity);

            await _loggerDB.UpdateLogDetails(logDetail, logId, CodesConstant.OK, LogsMessagesConstant.COMPLETED_SUCCESS);

            return ResponseHelper.ResponseSuccess<AccountDto>(CodesConstant.OK, $"{MessagesConstant.RECORD_FOUND} ", accountDto);
        }


        public async Task<ResponseDto<List<AccountDto>>> GetJustChildAccountListAsync()
        {
            Guid idPrueba = Guid.NewGuid();
            Guid logId = Guid.Empty;
            // Creacion del Detalle
            var logDetail = new LogDetailDto
            {
                Id = Guid.NewGuid(),
                EntityTableName = TablesConstant.ACCOUNT_CATALOG,
                EntityRowId = Guid.Empty,
                ChangeType = MessagesConstant.GET,
                OldValues = null,
                NewValues = null
            };

            // Creando el log
            var log = new LogCreateDto
            {
                UserId = idPrueba,
                ActionType = AcctionsConstants.DATA_GET,
                Status = CodesConstant.PENDING,
                Message = $"{LogsMessagesConstant.PENDING}",
                DetailId = logDetail.Id,
                ErrorId = null,
            };


            var childAccounts = await _context.AccountCatalogs
                .Where(account => account.IsActive == true && // Solo cuentas activas
                                  account.AllowsMovement == true && // Permiten movimiento
                                  account.ParentId != null && // La cuenta tiene un ParentId (es hija)
                                  !_context.AccountCatalogs.Any(subAccount => subAccount.ParentId == account.Id) // La cuenta no es padre de otras
                )
                .ToListAsync();

            // Verifica si existen cuentas hijas
            if (!childAccounts.Any())
            {
                await _loggerDB.LogStateUpdate(CodesConstant.OK, logId, $"{LogsMessagesConstant.NOT_FOUND}  ::  GET_DATA {TablesConstant.ACCOUNT_CATALOG}");
                return ResponseHelper.ResponseError<List<AccountDto>>(
                    CodesConstant.NOT_FOUND, $"{MessagesConstant.RECORD_NOT_FOUND}: No se encontraron cuentas hijas."
                );
            }
            logId = await _loggerDB.LogCreateLog(logDetail, log);

            // Mapea las cuentas hijas a AccountDto
            var accountDtos = _mapper.Map<List<AccountDto>>(childAccounts);
            await _loggerDB.LogStateUpdate(CodesConstant.OK, logId, $"{LogsMessagesConstant.COMPLETED_SUCCESS} =>>  GET_DATA {TablesConstant.ACCOUNT_CATALOG} : GetJustChildAccountListAsync()");


            return ResponseHelper.ResponseSuccess(
                CodesConstant.OK, $"{MessagesConstant.RECORD_FOUND}", accountDtos
            );
        }

        public async Task<ResponseDto<List<AccountDto>>> GetJustChildAccountInactiveListAsync()
        {
            var childAccounts = await _context.AccountCatalogs
            .Where(account => account.IsActive == false &&  // Solo cuentas inactivas
                              account.ParentId != null &&  // La cuenta tiene un ParentId (es hija)
                              !_context.AccountCatalogs.Any(subAccount => subAccount.ParentId == account.Id)  // No tiene subcuentas
            )
            .ToListAsync();

                // Verifica si existen cuentas hijas
                if (!childAccounts.Any())
                {
                    return ResponseHelper.ResponseError<List<AccountDto>>(
                        CodesConstant.NOT_FOUND, $"{MessagesConstant.RECORD_NOT_FOUND}: No se encontraron cuentas hijas inactivas."
                    );
                }

                // Mapea las cuentas hijas inactivas a AccountDto
                var accountDtos = _mapper.Map<List<AccountDto>>(childAccounts);

                return ResponseHelper.ResponseSuccess(
                    CodesConstant.OK, $"{MessagesConstant.RECORD_FOUND}", accountDtos
                );
        }


        public Task<ResponseDto<AccountDto>> EditAccountByIdAsync(AccountEditDto dto, Guid id, int maxDepth)
        {
            throw new NotImplementedException();
        }

    }
}
