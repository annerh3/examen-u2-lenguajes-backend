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
    }
}
