using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Databases.LogsDataBase;
using ProyectoExamenU2.Databases.LogsDataBase.Entities;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Dtos.Logs;
using ProyectoExamenU2.Helpers;
using ProyectoExamenU2.Services.Interfaces;
using System.Collections.Immutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProyectoExamenU2.Services
{
    public class LoggerDbService : ILoggerDBService
    {
        private readonly LogsContext _context;
        private readonly IMapper _mapper;
        private readonly int PAGE_SIZE;

        public LoggerDbService(
            LogsContext context,
            IMapper mapper,
             IConfiguration configuration
            )
        {
            _context = context;
            _mapper = mapper;
            PAGE_SIZE = configuration.GetValue<int>("PageSize");
        }

        public async Task<ResponseDto<PaginationDto<List<LogDto>>>> GetAllLogsWithDetailsAsync(string searchTerm = "", int page = 1, int codeStatus = 0)
        {
            int startIndex = (page - 1) * PAGE_SIZE;

            //IQueryable<LogEntity> logsEntityQuery = _context.Logs
            //        .Include(log => log.Detail)
            //        .Include(log => log.Error);

            IQueryable<LogEntity> logsEntityQuery = _context.Logs
                    .Include(log => log.Detail)
                    .Include(log => log.Error);

            Console.WriteLine($"Valor de codeStatus: {codeStatus}");

            if (codeStatus != 0)
            {
                logsEntityQuery = logsEntityQuery.Where(ml => ml.Status == codeStatus);
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                logsEntityQuery = logsEntityQuery.Where(leq => (leq.Message + " " + leq.ActionType + " " + leq.Status + " " + leq.Detail.EntityTableName + " " + leq.Detail.ChangeType + " " + leq.Error.ErrorMessage)
                    .ToLower().Contains(searchTerm.ToLower()));
            }

            if(!string.IsNullOrEmpty(searchTerm) && codeStatus !=0)
            {
                logsEntityQuery = logsEntityQuery.Where(leq => leq.Status == codeStatus
                                                           && (leq.Message + " " + leq.ActionType + " " + leq.Status + " " + leq.Detail.EntityTableName + " " + leq.Detail.ChangeType + " " + leq.Error.ErrorMessage)
                                                           .ToLower().Contains(searchTerm.ToLower()));          
            }

            int totalLogs = await logsEntityQuery.CountAsync();

            if (totalLogs == 0)
            {
                return new ResponseDto<PaginationDto<List<LogDto>>>
                {
                    StatusCode = 200,
                    Status = false,
                    Message = "No se encontraron logs que coincidan con los criterios de búsqueda",
                    Data = new PaginationDto<List<LogDto>>
                    {
                        CurrentPage = page,
                        PageSize = PAGE_SIZE,
                        TotalItems = 0,
                        TotalPages = 0,
                        Items = new List<LogDto>(),
                        HasPreviousPage = false,
                        HasNextPage = false,
                    }
                };
            }

            int totalPages = (int)Math.Ceiling((double)totalLogs / PAGE_SIZE);

            var logsEntity = await logsEntityQuery
                .OrderByDescending(l => l.Timestamp)
                .Skip(startIndex)
                .Take(PAGE_SIZE)
                .ToListAsync();

            var mappedLogs = _mapper.Map<List<LogDto>>(logsEntity);

            return new ResponseDto<PaginationDto<List<LogDto>>>
            {
                StatusCode = 200,
                Status = true,
                Message = "Listado de Logs Obtenida Correctamente",
                Data = new PaginationDto<List<LogDto>>
                {
                    CurrentPage = page,
                    PageSize = PAGE_SIZE,
                    TotalItems = totalLogs,
                    TotalPages = totalPages,
                    Items = mappedLogs,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages,
                }
            };

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
