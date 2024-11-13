using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using ProyectoExamenU2.Services.Interfaces;
using ProyectoExamenU2.Services;
using ProyectoExamenU2.Databases.LogsDataBase.Entities;

namespace ProyectoExamenU2.Databases.LogsDataBase
{
    public class LogsContext : DbContext
    {
        public LogsContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<LogEntity> Logs { get; set; }
        public DbSet<LogErrorEntity> LogsErrors { get; set; }
        public DbSet<LogDetailEntity> LogsDetails { get; set; }

    }
}
