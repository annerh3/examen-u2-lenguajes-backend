using AutoMapper;
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

        public AccountCatalogService(
            ProyectoExamenU2Context context,
            IMapper mapper
            )
        {
            this._context = context;
            this._mapper = mapper;
        }
        public async Task<ResponseDto<AccountDto>> CreateAcoountAsync(AccountCreateDto dto)
        {
            // mapp
            var AccountEntity = _mapper.Map<AccountCatalogEntity>(dto);


            // Vlidar que el Tipo de Dato sea 
            // D --> DEBIT o tambien como Debito o Debe
            // C --> CREDIT o tambien como CREDITO o Haber
            if (dto.BehaviorType != 'D' && dto.BehaviorType != 'C') return ResponseHelper.ResponseError<AccountDto>(
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


            //Proceder a Crear la Cuenta
            _context.AccountCatalogs.Add(AccountEntity);
            await _context.SaveChangesAsync();
            await _context.SaveChangesAsync();

            var AccountResult = _mapper.Map<AccountDto>(AccountEntity);
            // Respuesta
            string accountFullCode = $"{dto.PreCode}{dto.Code}";

            return ResponseHelper.ResponseSuccess<AccountDto>(
                CodesConstant.OK,
                $"{MessagesConstant.CREATE_SUCCESS} => Record: {AccountResult.Id}",
                AccountResult);
   
        }

        public Task<ResponseDto<AccountDto>> DeleteAccountByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<AccountDto>> EditAccountByIdAsync(AccountEditDto dto, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<AccountDto>> GetAccountByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<List<AccountDto>>> GetAccountListAsync()
        {
            throw new NotImplementedException();
        }
    }
}
