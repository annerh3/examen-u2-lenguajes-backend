using Microsoft.AspNetCore.Mvc;
using ProyectoExamenU2.Dtos.AccountCatalog;
using ProyectoExamenU2.Dtos.Journal;
using ProyectoExamenU2.Services;
using ProyectoExamenU2.Services.Interfaces;

namespace ProyectoExamenU2.Controllers
{
    [ApiController]
    [Route("api/journal-entry")]
    public class JournalEntryController :ControllerBase
    {
        private readonly IJournalService _journalService;

        public JournalEntryController(
            IJournalService journalService
            )
        {
            this._journalService = journalService;
        }

        [HttpPost]
        public async Task<ActionResult> CreateJournal(JournalEntryCreateDto dto)
        {

            var response = await _journalService.CreateJournalEntry(dto);
            return StatusCode(Response.StatusCode, response);
        }


    }
}
