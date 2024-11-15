using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Dtos.Logs;

namespace ProyectoExamenU2.Services.Interfaces
{
    public interface ILoggerDBService
    {
        Task<Guid> LogCreateLog(LogDetailDto logDetail, LogCreateDto logDto);
        Task UpdateLogDetails(LogDetailDto logDetail ,Guid id , int status , string message);
        // Errores
        Task LogError (Guid id , int status , LogErrorCreateDto dto, string message);

        Task LogStateUpdate(int  state , Guid id , string message );
        // La parte de Visualizacion del Usuario
        Task<IEnumerable<LogDto>> GetLogsAsync(DateTime fromDate, DateTime toDate, Guid userId, string actionType);
        Task<ResponseDto<PaginationDto<List<LogDto>>>> GetAllLogsWithDetailsAsync(string searchTerm = "", int page = 1, int codeStatus = 0);
    }
}
