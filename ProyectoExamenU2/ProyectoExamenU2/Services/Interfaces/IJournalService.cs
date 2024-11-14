using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Dtos.Journal;

namespace ProyectoExamenU2.Services.Interfaces
{
    public interface IJournalService
    {
        Task<ResponseDto<JournalDto>> CreateEvent(JournalEntryCreateDto dto);
    }
}
