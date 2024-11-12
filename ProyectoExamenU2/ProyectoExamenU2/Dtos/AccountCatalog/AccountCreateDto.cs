using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.AccountCatalog
{
    public class AccountCreateDto
    {
        [Display(Name = "Sufijo de Cuenta")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public string PreCode { get; set; }

        [Display(Name = "Codigo Primario")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public string Code { get; set; }

        [Display(Name = "Nombre de la Cuenta")]
        [Required(ErrorMessage = "El {0} es obligatoria.")]
        public string AccountName { get; set; }

        [Display(Name = "Comportamiento")]
        [Required(ErrorMessage = "El {0} es obligatoria.")]
        public char BehaviorType { get; set; }

        [Display(Name = "Permite Movimientos")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public bool AllowsMovement { get; set; }

        public Guid? ParentId { get; set; }
        public virtual ICollection<AccountCatalogEntity> ChildAccounts { get; set; } = new List<AccountCatalogEntity>();
    }
}
