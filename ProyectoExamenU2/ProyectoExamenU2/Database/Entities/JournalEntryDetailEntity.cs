using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Database.Entities
{
    [Table("journal_entry_detail", Schema = "dbo")]
    public class JournalEntryDetailEntity : BaseEntity
    {
        //Los campos de auditoria ya van anexados 

        [Display (Name ="Id de Partida")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("journal_entry_id")]
        public Guid JournalEntryId { get; set; }
        [ForeignKey(nameof(JournalEntryId))]
        public virtual JournalEntry JournalEntry { get; set; }

        [Display(Name = "Id de Cuenta")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("acoount_catalog_id")]
        public Guid AccountCatalogId { get; set; }
        [ForeignKey (nameof(AccountCatalogId))]

        public virtual AccountCatalog Account {  get; set; }

        [Display(Name = "Monto")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Tipo de Movimiento")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("entry_type")]
        public char EntryType { get; set; }
        // solo puede ser Cr o Dr
        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }

    }
}
