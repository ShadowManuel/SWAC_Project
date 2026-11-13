using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ticketapi.Daten;
using ticketapi.Models;
using ticketapi.Services;

namespace ticketapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {

        private readonly TicketDatenbank _db;
        private readonly LogClient _log;

        public TicketController(TicketDatenbank db, LogClient log)
        {
            _db = db;
            _log = log;
        }


        [HttpGet]
        public async Task<ActionResult<List<Ticket>>> GetOffeneTickets(int? level)
        {
            var abfrage = _db.Tickets.Where(t => t.Status == TicketStatus.Offen);

            if (level != null)
            {
                abfrage = abfrage.Where(t => t.Level == level);
            }

            var tickets = await abfrage.ToListAsync();
            return Ok(tickets);
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> TicketErstellen(TicketAnfrage anfrage)
        {
            var ticket = new Ticket
            {
                Titel = anfrage.Titel,
                Beschreibung = anfrage.Beschreibung,
                Ersteller = anfrage.Ersteller,
                Kategorie = anfrage.Kategorie.Value,
                Level = Kategorie.StartLevel(anfrage.Kategorie.Value),
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            await _log.LogAsync($"Ticket {ticket.Id} erstellt (Level {ticket.Level})");

            return Ok(ticket);
        }

        [HttpPost("{id}/eskalieren")]
        public async Task<ActionResult<Ticket>> Eskalieren(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            if (ticket.Level >= 3)
                return Conflict("Ticket ist bereits auf Level 3.");

            if (ticket.Status == TicketStatus.Erledigt)
                return Conflict("Ticket ist bereits erledigt.");

            ticket.Level = ticket.Level + 1;
            ticket.Status = TicketStatus.Offen;
            ticket.Bearbeiter = null;

            await _db.SaveChangesAsync();
            return Ok(ticket);
        }

        [HttpPost("{id}/uebernehmen")]
        public async Task<ActionResult<Ticket>> Uebernehmen(int id, UebernehmenAnfrage anfrage)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            if (ticket.Status != TicketStatus.Offen)
                return Conflict("Ticket ist nicht mehr offen.");

            ticket.Bearbeiter = anfrage.Bearbeiter;
            ticket.Status = TicketStatus.InBearbeitung;

            await _db.SaveChangesAsync();
            return Ok(ticket);
        }

        [HttpPost("{id}/erledigen")]
        public async Task<ActionResult<Ticket>> Erledigen(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            if (ticket.Status != TicketStatus.InBearbeitung)
                return Conflict("Nur Tickets in Bearbeitung können erledigt werden.");

            ticket.Status = TicketStatus.Erledigt;

            await _db.SaveChangesAsync();
            await _log.LogAsync($"Ticket {ticket.Id} abgeschlossen (Level {ticket.Level})");
            return Ok(ticket);
        }

        [HttpGet("meine")]
        public async Task<ActionResult<List<Ticket>>> GetMeineTickets(string bearbeiter)
        {
            var tickets = await _db.Tickets
                .Where(t => t.Bearbeiter == bearbeiter && t.Status == TicketStatus.InBearbeitung)
                .ToListAsync();

            return Ok(tickets);
        }
    }
}
