﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoExamenU2.Databases.PrincipalDataBase.Entities
{
    [Table("account_catalog", Schema = "dbo")]
    public class AccountCatalogEntity : BaseEntity
    {
        [Display(Name = "Sufijo de Cuenta")]
        //[Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("pre_code")]
        public string PreCode { get; set; }

        [Display(Name = "Codigo Primario")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("code")]
        public string Code { get; set; }

        [Display(Name = "Nombre de la Cuenta")]
        [Required(ErrorMessage = "El {0} es obligatoria.")]
        [Column("account_name")]
        public string AccountName { get; set; }

        [Display(Name = "Comportamiento")]
        [Required(ErrorMessage = "El {0} es obligatoria.")]
        [Column("behavior_type")]
        public char BehaviorType { get; set; }

        [Display(Name = "Permite Movimientos")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("allows_movement")]
        public bool AllowsMovement { get; set; }

        [Display(Name = "Es Activa")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("is_Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Cuenta Padre")]
        //[Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("parent_id")]
        public Guid? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        // Acceder a la cuenta padre
        public virtual AccountCatalogEntity ParentAccount { get; set; }

        // subcuentas si existen 
        public virtual ICollection<AccountCatalogEntity> ChildAccounts { get; set; } = new List<AccountCatalogEntity>();

        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }

    }
}
