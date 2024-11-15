using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Databases.LogsDataBase.Entities
{
    [Table("logs", Schema = "dbo")]
    public class LogEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }// va tocar crear un usuario interno llamado system 

        [Required]
        [StringLength(50)]
        [Column("action_type")]
        public string ActionType { get; set; }  

        [Required]
        [StringLength(20)]
        [Column("status")]
        public int Status { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("log_detail_id")]
        public  Guid DetailId { get; set; }
        [ForeignKey(nameof(DetailId))]

        public virtual LogDetailEntity Detail { get; set; }

        [Column("log_error_id")]
        public Guid? ErrorId { get; set; }
        [ForeignKey(nameof(ErrorId))]
        public virtual LogErrorEntity Error { get; set; }

    }
}
