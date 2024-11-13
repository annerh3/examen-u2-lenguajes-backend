using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Common;
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
            // Validando si existe el padre 
            // verifica si el pre_code+code del padre es igual al pre_code del hijo
            // Lo hize con ternario para ahorar lineas mucho tramite 
            var parentAccount = dto.ParentId.HasValue
                ? await _context.AccountCatalogs.FirstOrDefaultAsync(a => a.Id == dto.ParentId.Value)
                : null;
            if (AccountEntity.ParentId.HasValue)
            {
                //var parentAccount = await _context.AccountCatalogs.FirstOrDefaultAsync(account => account.Id == dto.ParentId.Value);

                if (parentAccount == null)
                {
                    // Error por que el Padre no existe 
                    return new ResponseDto<AccountDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.RECORD_NOT_FOUND} : {dto.ParentId}",
                        Data = null
                    };
                }

                // Revisando l Pre Code
                string parentCompleteCode = $"{parentAccount.PreCode}{parentAccount.Code}";
                if (parentCompleteCode != dto.PreCode)
                {
                    // El Pre codigo del Hijo no coincide con el Codigo Completo del Padre
                    return new ResponseDto<AccountDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.PreCodeMismatch}: Parent = {parentCompleteCode} , Accoun-PreCode {dto.PreCode} ",
                        Data = null
                    };
                }
            }

            // Validacion que no Exista una cuenta Igual
            // se usa el PreCode+Code para ello
           // var codeComplete = $"{dto.PreCode}{dto.Code}";
            var existingAccount = await _context.AccountCatalogs.
                FirstOrDefaultAsync( account => (account.PreCode == dto.PreCode)  && (account.Code == dto.Code ));

            // si es nulo no existe ninguna
            // si no es nulo existe almenos 1
            if (existingAccount != null)
            {
                // error la cuenta ya existe
                return new ResponseDto<AccountDto>
                {
                    StatusCode = 400,
                    Status = false,
                    Message = $"{MessagesConstant.CREATE_ERROR} => {MessagesConstant.AccountAlreadyExists} : {existingAccount.AccountName}",
                    Data = null
                };
            }

            //Proceder a Crear la Cuenta
            _context.AccountCatalogs.Add(AccountEntity);
            await _context.SaveChangesAsync();
            await _context.SaveChangesAsync();
            var AccountResult = _mapper.Map<AccountDto>(AccountEntity);
            // Respuesta
            string accountFullCode = $"{dto.PreCode}{dto.Code}";

            return new ResponseDto<AccountDto>
            {
                StatusCode = 200,
                Status = true,
                Message = $"{MessagesConstant.CREATE_SUCCESS}",
                Data = AccountResult
            };
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
