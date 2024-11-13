using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProyectoExamenU2.Databases.LogsDataBase.Entities;

namespace ProyectoExamenU2.Dtos.Logs
{
    public class LogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid UserId { get; set; }// va tocar crear un usuario interno llamado system 
        public string ActionType { get; set; }
        public string Status { get; set; }
        public  LogDetailDto Detail { get; set; }
        public LogErrorDto Error { get; set; }
    }
}
