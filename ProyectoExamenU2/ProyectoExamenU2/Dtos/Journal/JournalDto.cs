using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProyectoExamenU2.Dtos.Journal.DtosHelper;

namespace ProyectoExamenU2.Dtos.Journal
{
    public class JournalDto
    {
        public Guid Id { get; set; }
        public int EntryNumber { get; set; }
        // definido como int pero puede cambiar a string segun mas adelante 
        public string Description { get; set; }
        public DateTime Date { get; set; }

        // agrgando una lista de evendetaids 
        public virtual ICollection<JournalEntryDetailDto> JournalEntryDetails { get; set; } = new List<JournalEntryDetailDto>();
    }
}
