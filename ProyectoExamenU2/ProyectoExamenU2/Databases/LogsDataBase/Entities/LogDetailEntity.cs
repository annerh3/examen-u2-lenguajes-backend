using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Databases.LogsDataBase.Entities
{
    public class LogDetailEntity
    {
        [Required]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column ("tableAfected")]
        public string EntityTableName { get; set; }

        [Column("row_id")]
        [Required]
        public Guid EntityRowId { get; set; }
        [Column("change_type")]
        public string ChangeType { get; set; }

        [Column("old_values")]
        public string OldValues { get; set; }
        [Column("new_values")]
        public string NewValues { get; set; }
    }
}
