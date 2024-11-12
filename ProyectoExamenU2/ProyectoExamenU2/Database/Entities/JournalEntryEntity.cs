using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Database.Entities
{
    [Table("journal_entry", Schema = "dbo")]
    public class JournalEntryEntity:BaseEntity
    {
        [Display(Name = "N Partida")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("entry_number")]
        public int EntryNumber { get; set; }
        // definido como int pero puede cambiar a string segun mas adelante 


        [Display(Name = "Description")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [MaxLength(600 , ErrorMessage = "El maximo de Caracteres es de {1}")]
        [MinLength(10, ErrorMessage ="El minimo de Caracteres es de {2}")]
        [Column("description")]
        public string Description { get; set; }

        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("date")]
        public  DateTime Date { get; set; }

        // agrgando una lista de evendetaids 
        public virtual ICollection<JournalEntryDetailEntity> JournalEntryDetails { get; set; } = new List<JournalEntryDetailEntity>();
        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }
    }
}
