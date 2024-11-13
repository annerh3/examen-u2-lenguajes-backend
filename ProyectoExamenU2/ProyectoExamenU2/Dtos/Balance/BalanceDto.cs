using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProyectoExamenU2.Dtos.AccountCatalog;

namespace ProyectoExamenU2.Dtos.Balance
{
    public class BalanceDto
    {
        public Guid Id { get; set; }
        public AccountDto AccountCatalog { get; set; }
        public decimal BalanceAmount { get; set; }
        public DateTime Date { get; set; }

    }
}
