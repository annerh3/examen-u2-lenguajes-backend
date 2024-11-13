using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Dtos.Logs
{
    public class LogErrorDto
    {
        public Guid Id { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
