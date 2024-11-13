using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.Logs
{
    public class LogDetailCreateDto
    {
        public string EntityTableName { get; set; }
        public Guid EntityRowId { get; set; }
        public string ChangeType { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}
