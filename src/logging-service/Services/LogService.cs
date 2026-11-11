using logging_service.Models;
using MongoDB.Driver;

namespace logging_service.Services;

public class LogService
{
    private readonly IMongoCollection<LogEntry> _logs;

    public LogService(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("LogDb")
                               ?? throw new InvalidOperationException("ConnectionString LogDb fehlt.");

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(3);

        var client = new MongoClient(settings);
        var database = client.GetDatabase(config["Mongo:Database"] ?? "sims_logs");
        _logs = database.GetCollection<LogEntry>("logs");
    }

    public Task AddAsync(LogEntry entry, CancellationToken ct) =>
        _logs.InsertOneAsync(entry, cancellationToken: ct);

    public Task<List<LogEntry>> GetLatestAsync(int limit, CancellationToken ct) =>
        _logs.Find(FilterDefinition<LogEntry>.Empty)
            .Sort(Builders<LogEntry>.Sort.Descending(l => l.Id))
            .Limit(limit)
            .ToListAsync(ct);
}