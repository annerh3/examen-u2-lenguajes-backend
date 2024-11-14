using AutoMapper;
using ProyectoExamenU2.Databases.LogsDataBase;
using ProyectoExamenU2.Databases.LogsDataBase.Entities;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Dtos.Logs;
using ProyectoExamenU2.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProyectoExamenU2.Services
{
    public class LoggerDbService : ILoggerDBService
    {
        private readonly LogsContext _context;
        private readonly IMapper _mapper;

        public LoggerDbService(
            LogsContext context,
            IMapper mapper
            )
        {
            this._context = context;
            this._mapper = mapper;
        }

        public Task<IEnumerable<LogDto>> GetLogsAsync(DateTime fromDate, DateTime toDate, Guid userId, string actionType)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> LogCreateLog(LogDetailDto logDetail, LogCreateDto logDto)
        {
            var detailEntity = _mapper.Map<LogDetailEntity>(logDetail);

            await _context.LogsDetails.AddAsync(detailEntity);

            var logEntity = _mapper.Map<LogEntity>(logDto);
            logEntity.Timestamp = DateTime.UtcNow;

            await _context.Logs.AddAsync(logEntity);
            await _context.SaveChangesAsync();
            return  logEntity.Id;
            

        }

        public async Task LogError(Guid id, int status, LogErrorCreateDto dto, string message)
        {
            var logErrorEntity = _mapper.Map<LogErrorEntity>(dto);
            logErrorEntity.TimeStamp = DateTime.UtcNow;
            await _context.LogsErrors.AddAsync(logErrorEntity);
            await _context.SaveChangesAsync();

            var logEntity = await _context.Logs.FindAsync(id);
            logEntity.Status = status;
            logEntity.Message = message;
            
            logEntity.ErrorId = logErrorEntity.Id;

            _context.Logs.Entry(logEntity);

            await _context.SaveChangesAsync();

        }

        public async Task LogStateUpdate(int state, Guid id , string message)
        {
            var logEntity = await _context.Logs.FindAsync(id);
            logEntity.Status= state;
            logEntity.Message= message;

            _context.Logs.Entry(logEntity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLogDetails(LogDetailDto logDetail, Guid id, int status , string message)
        {
            //log details se busca y actualiza
            var detailEntity = await _context.LogsDetails.FindAsync(logDetail.Id);
            detailEntity = _mapper.Map(logDetail, detailEntity);

            _context.LogsDetails.Entry(detailEntity);
            await _context.SaveChangesAsync();


            // log Primario se actualiza el estado
            var logEntity = await _context.Logs.FindAsync(id);
            logEntity.Status = status;
            logEntity.Message = message;
            _context.Logs.Entry(logEntity);

            await _context.SaveChangesAsync();

   
        }
    }
}
