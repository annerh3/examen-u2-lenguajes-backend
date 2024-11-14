using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.Journal.DtosHelper
{
    public class AccountEntry
    {
        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public decimal amount { get; set; }

        [Required]
        public char muvType { get; set; }
    }
}
