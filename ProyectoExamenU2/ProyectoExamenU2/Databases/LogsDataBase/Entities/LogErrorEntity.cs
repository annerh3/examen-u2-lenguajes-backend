using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Databases.LogsDataBase.Entities
{
    public class LogErrorEntity
    {
        [Column("id")]
        public Guid Id { get; set; }
        [Column("error_code")]
        public string ErrorCode { get; set; }
        [Column("error_message")]
        public string ErrorMessage { get; set; }

        [Column("stack_trace")]
        public string StackTrace { get; set; }
        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }
    }
}
