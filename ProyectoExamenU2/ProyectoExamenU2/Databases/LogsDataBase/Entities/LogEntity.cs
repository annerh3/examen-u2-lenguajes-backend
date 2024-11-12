using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Databases.LogsDataBase.Entities
{
    [Table("logs", Schema = "dbo")]
    public class LogEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("action_type")]
        public string ActionType { get; set; }  // Tipo de acción realizada (ej. "Crear", "Modificar", "Eliminar", "Autenticación")

        [Required]
        [StringLength(100)]
        [Column("entity_name")]
        public string EntityName { get; set; } // Nombre de la entidad afectada (ej. "PartidaContable", "Cuenta", etc.)

        [Column("entity_id")]
        public Guid EntityId { get; set; } // ID del registro de la entidad que fue afectado

        [StringLength(500)]
        [Column("details")]
        public string Details { get; set; } // Detalles adicionales (ej. valores antiguos y nuevos, si es una actualización)

        [Required]
        [StringLength(20)]
        [Column("status")]
        public string Status { get; set; } // Estado de la acción (ej. "Éxito", "Error")

    }
}
