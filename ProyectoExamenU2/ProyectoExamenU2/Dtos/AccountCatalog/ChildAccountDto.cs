namespace ProyectoExamenU2.Dtos.AccountCatalog
{
    public class ChildAccountDto
    {
        public Guid Id { get; set; }
        public string FullCode { get; set; }
        public string AccountName { get; set; }
        public char BehaviorType { get; set; }
        public bool AllowsMovement { get; set; }
        public bool IsActive { get; set; }


        public ICollection<ChildAccountDto> ChildAccounts { get; set; } = new List<ChildAccountDto>();
    }
}
