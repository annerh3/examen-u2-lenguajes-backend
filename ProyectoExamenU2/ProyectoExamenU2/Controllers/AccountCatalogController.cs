using Microsoft.AspNetCore.Mvc;
using ProyectoExamenU2.Dtos.AccountCatalog;
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
        public async Task<ActionResult> CreateNote(AccountCreateDto dto)
        {

            var response = await _accountCatalogService.CreateAcoountAsync(dto);
            return StatusCode(Response.StatusCode, response);
        }

    }
}
