using ProyectoExamenU2.Dtos.Logs;

namespace ProyectoExamenU2.Services.Interfaces
{
    public interface ILoggerDBService
    {
        // acciones
        Task LogCreateActionAsync(LogCreateDto dto);

        // Errores
        Task LogCreateErrorAsync(Guid userId, string errorMessage, string stackTrace, Guid LogEntityId);

        // Cambios
        Task LogEntityChangeAsync(Guid userId, string entityName, Guid entityId, string changeType,string oldValues, string newValues);

        // La parte de Visualizacion del Usuario
        Task<IEnumerable<LogDto>> GetLogsAsync(DateTime fromDate, DateTime toDate, Guid userId, string actionType);
    }
}
