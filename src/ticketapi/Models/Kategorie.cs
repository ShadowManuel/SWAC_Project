namespace ticketapi.Models
{
    public static class Kategorie
    {
        public static int StartLevel(TicketKategorie kategorie) => kategorie switch
        {
            TicketKategorie.Logging => 1,
            TicketKategorie.Connecting => 1,
            TicketKategorie.Other => 1,
            TicketKategorie.Phishing => 2,
            TicketKategorie.Malware => 2,
            _ => 1,
        };

    }
}
