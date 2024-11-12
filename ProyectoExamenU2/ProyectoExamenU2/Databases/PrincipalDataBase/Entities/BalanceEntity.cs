using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Databases.PrincipalDataBase.Entities
{
    [Table("balances", Schema = "dbo")]
    public class BalanceEntity : BaseEntity
    {
        [Display(Name = "Id de Cuenta")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("account_catalog_id")]
        public Guid AccountCatalogId { get; set; }
        [ForeignKey(nameof(AccountCatalogId))]

        public virtual AccountCatalogEntity AccountCatalog { get; set; }

        [Display(Name = "Monto del Balance")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("balance_amount")]

        public decimal BalanceAmount { get; set; }

        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("date")]
        public DateTime Date { get; set; }

        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }
    }
}
