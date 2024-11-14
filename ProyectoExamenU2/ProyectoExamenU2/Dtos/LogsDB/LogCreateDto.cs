using ProyectoExamenU2.Databases.LogsDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.Logs
{
    public class LogCreateDto
    {
        public Guid UserId { get; set; }// va tocar crear un usuario interno llamado system 
        public string ActionType { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public Guid DetailId { get; set; }
        public Guid? ErrorId { get; set; }
    }
}
