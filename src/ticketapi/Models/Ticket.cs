namespace ticketapi.Models
{

    public enum TicketStatus { Offen, InBearbeitung, Erledigt }

    public enum TicketKategorie { Phishing, Logging, Malware, Connecting, Other }

    public class Ticket
    {
        public int Id { get; set; }

        public string Titel { get; set; } = "";

        public string Beschreibung { get; set; } = "";

        public string Ersteller { get; set; } = "";

        public string? Bearbeiter { get; set; }

        public TicketKategorie Kategorie { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.Offen;

        public int Level { get; set; } = 1;

        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

    }
}
