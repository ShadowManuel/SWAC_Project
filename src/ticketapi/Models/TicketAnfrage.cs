using System.ComponentModel.DataAnnotations;

namespace ticketapi.Models
{
    public class TicketAnfrage
    {
        [Required]
        public string Titel { get; set; } = "";

        public string Beschreibung { get; set; } = "";

        [Required]
        public TicketKategorie? Kategorie { get; set; }

        [Required]

        public string Ersteller { get; set; } = "";
    }
}


