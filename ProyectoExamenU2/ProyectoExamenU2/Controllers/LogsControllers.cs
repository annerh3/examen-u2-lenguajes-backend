using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Dtos.Logs;
using ProyectoExamenU2.Services.Interfaces;

namespace ProyectoExamenU2.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogsControllers : ControllerBase
    {
        private readonly ILoggerDBService _loggerDBService;

        public LogsControllers(ILoggerDBService loggerDBService)
        {
            this._loggerDBService = loggerDBService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<PaginationDto<List<LogDto>>>>> GetAllLogsPaginated(string searchTerm = "", int page = 1, int codeStatus = 0)
        {
            var response = await _loggerDBService.GetAllLogsWithDetailsAsync(searchTerm, page, codeStatus);
            return StatusCode(response.StatusCode, response);
        }
    }
}
