using Microsoft.EntityFrameworkCore;
using ticketapi.Models;

namespace ticketapi.Daten
{
    public class TicketDatenbank : DbContext
    {
        public TicketDatenbank(DbContextOptions<TicketDatenbank> options) : base(options) { }

        public DbSet<Ticket> Tickets { get; set; }
    }
}
