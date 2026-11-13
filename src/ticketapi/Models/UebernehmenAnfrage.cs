using System.ComponentModel.DataAnnotations;

namespace ticketapi.Models
{
    public class UebernehmenAnfrage
    {
        [Required]
        public string Bearbeiter { get; set; } = "";
    }
}