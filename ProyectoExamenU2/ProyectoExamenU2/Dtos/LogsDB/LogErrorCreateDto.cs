using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Dtos.Logs
{
    public class LogErrorCreateDto
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }

        public string TargetSite { get; set; }
    }
}
