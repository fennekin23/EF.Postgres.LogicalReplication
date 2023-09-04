using EF.Postgres.LogicalReplication.Example.Storage;

namespace EF.Postgres.LogicalReplication.Example.Handlers;

public sealed partial class InsertBookHandler(ILogger<InsertBookHandler> logger) : IInsertHandler<Book>
{
    public Task<bool> Handle(Book entity, CancellationToken cancellationToken = default)
    {
        Log.EntityReceived(logger, entity.Id, entity.Title);

        return Task.FromResult(true);
    }

    public partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Book entity received, id: {Id}, title: '{Title}'.")]
        public static partial void EntityReceived(ILogger logger, int id, string title);
    }
}
