using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace logging_service.Models;

public class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; }

    public string Level { get; set; } = "Info";
    public string Service { get; set; } = "";
    public string Message { get; set; } = "";
}