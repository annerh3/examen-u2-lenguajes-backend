using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProyectoExamenU2.Dtos.Journal.DtosHelper;

namespace ProyectoExamenU2.Dtos.Journal
{
    public class JournalEntryCreateDto
    {

        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [MaxLength(600, ErrorMessage = "El maximo de caracteres es de {1}.")]
        [MinLength(10, ErrorMessage = "El mínimo de caracteres es de {1}.")]
        public string Description { get; set; }

        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public DateTime Date { get; set; }

        // agrgando una lista de evendetaids 
        public virtual ICollection<AccountEntry> AccountsEntrys { get; set; }

    }
}
