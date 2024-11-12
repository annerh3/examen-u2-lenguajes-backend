using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Databases.PrincipalDataBase.Entities
{
    public class AuditEntity
    {
        // Campos de Auditoría
        [StringLength(450)]
        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [StringLength(450)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; }

        [Column("updated_date")]
        public DateTime UpdatedDate { get; set; }
    }
}
