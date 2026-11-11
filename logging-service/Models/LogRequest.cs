using System.ComponentModel.DataAnnotations;

namespace logging_service.Models;

public class LogRequest
{
    [Required, StringLength(20)]
    public string Level { get; set; } = "";

    [Required, StringLength(100)]
    public string Service { get; set; } = "";

    [Required, StringLength(2000)]
    public string Message { get; set; } = "";
}