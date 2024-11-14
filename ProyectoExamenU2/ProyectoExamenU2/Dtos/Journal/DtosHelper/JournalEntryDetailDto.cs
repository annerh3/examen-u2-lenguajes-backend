using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.Journal.DtosHelper
{
    public class JournalEntryDetailDto
    {
        public Guid Id { get; set; }
        public Guid JournalEntryId { get; set; }
        //elimine el virtual
        public Guid AccountCatalogId { get; set; }
        public virtual AccountCatalogEntity Account { get; set; }
        public decimal Amount { get; set; }
        public char EntryType { get; set; }

    }
}
