using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProyectoExamenU2.Dtos.Journal.DtosHelper;

namespace ProyectoExamenU2.Dtos.Journal
{
    public class JournalEntryCreateDto
    {

        [Display(Name = "Description")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [MaxLength(600, ErrorMessage = "El maximo de Caracteres es de {1}")]
        [MinLength(10, ErrorMessage = "El minimo de Caracteres es de {2}")]
        [Column("description")]
        public string Description { get; set; }

        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("date")]
        public DateTime Date { get; set; }

        // agrgando una lista de evendetaids 
        public virtual ICollection<AccountEntry> AccountsEntrys { get; set; }

    }
}
