namespace ProyectoExamenU2.Dtos.Logs
{
    public class LogDetailDto
    {
        public Guid Id { get; set; }
        public string EntityTableName { get; set; }
        public Guid? EntityRowId { get; set; }
        public string ChangeType { get; set; }
        public dynamic OldValues { get; set; }
        public dynamic NewValues { get; set; }
    }
}
