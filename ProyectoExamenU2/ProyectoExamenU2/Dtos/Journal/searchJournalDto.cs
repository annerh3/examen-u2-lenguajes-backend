namespace ProyectoExamenU2.Dtos.Journal
{
    public class searchJournalDto
    {
            public List<Guid>? Guids { get; set; } 
            public string SearchTerm { get; set; } = string.Empty;
            public int Page { get; set; } = 1;
    }
}
