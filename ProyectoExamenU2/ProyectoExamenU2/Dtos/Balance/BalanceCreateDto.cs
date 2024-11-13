using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.Balance
{
    public class BalanceCreateDto
    {
        [Display(Name = "Id de Cuenta")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public Guid AccountCatalogId { get; set; }
        [Display(Name = "Monto del Balance")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public decimal BalanceAmount { get; set; }

        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public DateTime Date { get; set; }
    }
}
