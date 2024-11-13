namespace ProyectoExamenU2.Dtos.Logs
{
    public class LogDetailDto
    {
        public int Id { get; set; }
        public string EntityTableName { get; set; }
        public Guid EntityRowId { get; set; }
        public string ChangeType { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}
