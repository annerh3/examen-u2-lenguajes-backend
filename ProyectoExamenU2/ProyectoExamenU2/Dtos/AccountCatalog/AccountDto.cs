﻿using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.AccountCatalog
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string PreCode { get; set; }

        public string Code { get; set; }

        public string AccountName { get; set; }

        public char BehaviorType { get; set; }
        public bool AllowsMovement { get; set; }
        //public Guid? ParentId { get; set; }
        public virtual AccountCatalogEntity ParentAccount { get; set; }
        public virtual ICollection<AccountCatalogEntity> ChildAccounts { get; set; } = new List<AccountCatalogEntity>();
    }
}