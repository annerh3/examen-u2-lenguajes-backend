using Microsoft.AspNetCore.Mvc;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Services.Interfaces;

namespace ProyectoExamenU2.Controllers
{
    [ApiController]
    [Route("api/Accounts")]
    public class AccountCatalogController:ControllerBase
    {
        private readonly IAccountCatalogService _accountCatalogService;

        public AccountCatalogController(
            IAccountCatalogService accountCatalogService
            )
        {
            this._accountCatalogService = accountCatalogService;
        }
        //Realizaciones

        [HttpPost]
        public async Task<ActionResult> CreateAccount(AccountCreateDto dto)
        {

            var response = await _accountCatalogService.CreateAcoountAsync(dto);
            return StatusCode(Response.StatusCode, response);
        }


        [HttpPut("{Id}")]
        public async Task<ActionResult<ResponseDto<AccountDto>>> EditAccount(AccountEditDto dto, Guid id)
        {
            var response = await _accountCatalogService.EditAccountByIdAsync(dto, id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<ResponseDto<AccountDto>>> GetById(Guid id)
        {
            var response = await _accountCatalogService.GetAccountByIdAsync(id );
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("childs")]
        public async Task<ActionResult<ResponseDto<AccountDto>>> GetChilds()
        {
            var response = await _accountCatalogService.GetJustChildAccountListAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("childs/inactive")]
        public async Task<ActionResult<ResponseDto<AccountDto>>> GetChildsInactives()
        {
            var response = await _accountCatalogService.GetJustChildAccountInactiveListAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}
