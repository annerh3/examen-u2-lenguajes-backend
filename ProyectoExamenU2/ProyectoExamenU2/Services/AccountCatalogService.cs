using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Helpers;
using ProyectoExamenU2.Services.Interfaces;

namespace ProyectoExamenU2.Services
{
    public class AccountCatalogService : IAccountCatalogService
    {
        private readonly ProyectoExamenU2Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<IAccountCatalogService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBalanceService _balanceService;

        public AccountCatalogService(
            ProyectoExamenU2Context context,
            IMapper mapper,
            ILogger<IAccountCatalogService> logger,
            IConfiguration configuration,
            IBalanceService balanceService
            )
        {
            this._context = context;
            this._mapper = mapper;
            this._logger = logger ;
            this._configuration = configuration;
            this._balanceService = balanceService;
        }
        public async Task<ResponseDto<AccountDto>> CreateAcoountAsync(AccountCreateDto dto)
        {
            // mapp
            var AccountEntity = _mapper.Map<AccountCatalogEntity>(dto);
            AccountEntity.IsActive = true;

            // Vlidar que el Tipo de Dato sea 
            // D --> DEBIT o tambien como Debito o Debe
            // C --> CREDIT o tambien como CREDITO o Haber
            if (dto.BehaviorType.ToString().ToUpper() != "D" && dto.BehaviorType.ToString().ToUpper() != "C") return ResponseHelper.ResponseError<AccountDto>(
                    CodesConstant.BAD_REQUEST,
                    $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.BehaviorTypeError} ");

            // Validacion que no Exista una cuenta Igual
            // se usa el PreCode+Code para ello
            // var codeComplete = $"{dto.PreCode}{dto.Code}";
            var existingAccount = await _context.AccountCatalogs.
                FirstOrDefaultAsync(account => (account.PreCode == dto.PreCode) && (account.Code == dto.Code));

            // si es nulo no existe ninguna
            // si no es nulo existe almenos 1
            if (existingAccount != null) return ResponseHelper.ResponseError<AccountDto>(
                    CodesConstant.CONFLICT,
                    $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.AccountAlreadyExists} : {existingAccount.AccountName}");

            // Validando si existe el padre 
            // verifica si el pre_code+code del padre es igual al pre_code del hijo
            // Lo hize con ternario para ahorar lineas mucho tramite 
            var parentAccount = dto.ParentId.HasValue
                ? await _context.AccountCatalogs.FirstOrDefaultAsync(a => a.Id == dto.ParentId.Value)
                : null;
            if (AccountEntity.ParentId.HasValue)
            {
                //var parentAccount = await _context.AccountCatalogs.FirstOrDefaultAsync(account => account.Id == dto.ParentId.Value);

                if (parentAccount == null) return ResponseHelper.ResponseError<AccountDto>(
                        CodesConstant.BAD_REQUEST,
                        $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.RECORD_NOT_FOUND} : {dto.ParentId}"
                        );
                // Revisando l Pre Code
                string parentCompleteCode = $"{parentAccount.PreCode}{parentAccount.Code}";
                if (parentCompleteCode != dto.PreCode) return ResponseHelper.ResponseError<AccountDto>(
                        CodesConstant.BAD_REQUEST,
                        $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.PreCodeMismatch}: Parent = {parentCompleteCode} , Accoun-PreCode {dto.PreCode} "
                        );

                // Si el padre tiene hijos el movimiento se bloquea
                // no se si tendremos un estado a validar ejemplo partida dada de baja pero sigue siendo hija
                if (parentAccount.ChildAccounts == null)
                {
                    parentAccount.ChildAccounts = new List<AccountCatalogEntity>();
                }

                // Agregar la nueva cuenta hija al padre
                parentAccount.ChildAccounts.Add(AccountEntity);

                // Accrualiza ya que ahora cuenta con almenos 1 hija
                parentAccount.AllowsMovement = false;

                // Modifica el Registro del Padre
                _context.AccountCatalogs.Update(parentAccount);

            }
            // TODO VERIFICAR SI EXISTIA ALGUN SALDO EN LA CUENTA PADRE CUENDO NO TENIA HIJAS 
            // Y ASIGNARLO A LA CUENTA HIJA CREADA EN ESE MOMENTO Y REGISTRAR LA PARTIDA 
            // 


            //Proceder a Crear la Cuenta
            _context.AccountCatalogs.Add(AccountEntity);
            await _context.SaveChangesAsync();

            var result =  await _balanceService.CreateInitBalance(AccountEntity.Id);



            var AccountResult = _mapper.Map<AccountDto>(AccountEntity);
            // Respuesta
            string accountFullCode = $"{dto.PreCode}{dto.Code}";

            return ResponseHelper.ResponseSuccess<AccountDto>(
                CodesConstant.OK,
                $"{MessagesConstant.CREATE_SUCCESS} => Record: {AccountResult.Id} :: Name :{AccountResult.AccountName} ",
                AccountResult);
   
        }

        public Task<ResponseDto<AccountDto>> DowAccountByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto<AccountDto>> EditAccountByIdAsync(AccountEditDto dto, Guid id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Busca la entidad en la base de datos
                    var existingAccount = await _context.AccountCatalogs.FirstOrDefaultAsync(a => a.Id == id);
                    // si no la encuentra
                    if (existingAccount is null)
                        return ResponseHelper.ResponseError<AccountDto>(
                            CodesConstant.NOT_FOUND,
                            $"{MessagesConstant.UPDATE_ERROR} => {MessagesConstant.RECORD_NOT_FOUND} : {id}");

                    // Validacion del Tipo de Movimeinto
                    if ( dto.BehaviorType.ToString().ToUpper() != "D" && dto.BehaviorType.ToString().ToUpper() != "C")
                        return ResponseHelper.ResponseError<AccountDto>(
                            CodesConstant.BAD_REQUEST,
                            $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.BehaviorTypeError}");

                    //  PreCode+Code para duplicados
                    //var duplicateAccount = await _context.AccountCatalogs.FirstOrDefaultAsync(account =>
                    //    account.PreCode == dto.PreCode && account.Code == dto.Code && account.Id != id);
                    //if (duplicateAccount != null)
                    //    return ResponseHelper.ResponseError<AccountDto>(
                    //        CodesConstant.CONFLICT,
                    //        $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.AccountAlreadyExists} : {duplicateAccount.AccountName}");

                    // Mapeo de la entidad a la nueva 
                    //
                    var entityMap = _mapper.Map(dto, existingAccount);

                    // Logica para los padres
                    if (existingAccount.ParentId != dto.ParentId)
                    {
                        // Elimina la cuenta actual hija del padre anterior, si tenia padre
                        if (existingAccount.ParentId.HasValue)
                        {
                            var oldParent = await _context.AccountCatalogs.FirstOrDefaultAsync(a => a.Id == existingAccount.ParentId.Value);
                            if (oldParent != null && oldParent.ChildAccounts != null)
                            {
                                oldParent.ChildAccounts.Remove(existingAccount);
                                oldParent.AllowsMovement = !oldParent.ChildAccounts.Any();
                                _context.AccountCatalogs.Update(oldParent);
                            }
                        }

                        // asiga un nuevo padre si existe
                        if (dto.ParentId.HasValue)
                        {
                            var newParent = await _context.AccountCatalogs.FirstOrDefaultAsync(a => a.Id == dto.ParentId.Value);
                            if (newParent == null)
                                return ResponseHelper.ResponseError<AccountDto>(
                                    CodesConstant.BAD_REQUEST,
                                    $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.RECORD_NOT_FOUND} : {dto.ParentId}");

                            // Valida el Pre Code de el nuevo padre 
                            var newParentCode = $"{newParent.PreCode}{newParent.Code}";
                            if (newParentCode != dto.PreCode)
                                return ResponseHelper.ResponseError<AccountDto>(
                                    CodesConstant.BAD_REQUEST,
                                    $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.PreCodeMismatch}: Parent = {newParentCode} , Account-PreCode = {dto.PreCode}");

                            // Actualiza las relaciones de Hijos en El nuevo Padre
                            if (newParent.ChildAccounts == null) newParent.ChildAccounts = new List<AccountCatalogEntity>();
                            newParent.ChildAccounts.Add(existingAccount);
                            newParent.AllowsMovement = false;
                            _context.AccountCatalogs.Update(newParent);
                        }
                    }

                    // Guarda los cambios en la base de datos
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Retorna el DTO 
                    var result = _mapper.Map<AccountDto>(existingAccount);
                    return ResponseHelper.ResponseSuccess<AccountDto>(
                        CodesConstant.OK,
                        $"{MessagesConstant.UPDATE_SUCCESS} => Record : {id}", result);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "Error al editar la cuenta en el try.");
                    return ResponseHelper.ResponseError<AccountDto>(
                        CodesConstant.INTERNAL_SERVER_ERROR,
                        $"{MessagesConstant.UPDATE_ERROR} => {MessagesConstant.API_FATAL_ERROR} : Record {id}");
                }
            }
        }

        public async Task<ResponseDto<AccountDto>> GetAccountByIdAsync(Guid id)
        {
            var AccountEntity = await _context.AccountCatalogs
                    .Include(account => account.ChildAccounts) // Incluye las cuentas hijas de primer nivel
                    .FirstOrDefaultAsync(ev => ev.Id == id);


            if (AccountEntity == null)
                return ResponseHelper.ResponseError<AccountDto>(CodesConstant.NOT_FOUND, $"{MessagesConstant.RECORD_NOT_FOUND} : Record {id}");
            var accountDto = _mapper.Map<AccountDto>(AccountEntity);

            return ResponseHelper.ResponseSuccess<AccountDto>(CodesConstant.OK, $"{MessagesConstant.RECORD_FOUND} ", accountDto);
        }


        public async Task<ResponseDto<List<AccountDto>>> GetJustChildAccountListAsync()
        {
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
                return ResponseHelper.ResponseError<List<AccountDto>>(
                    CodesConstant.NOT_FOUND, $"{MessagesConstant.RECORD_NOT_FOUND}: No se encontraron cuentas hijas."
                );
            }

            // Mapea las cuentas hijas a AccountDto
            var accountDtos = _mapper.Map<List<AccountDto>>(childAccounts);

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
