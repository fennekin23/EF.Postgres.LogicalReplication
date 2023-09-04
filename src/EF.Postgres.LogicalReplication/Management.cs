using Npgsql;

namespace EF.Postgres.LogicalReplication;

internal class Management(string connectionString)
{
    public async Task CreatePublication(string publicationName, string tableName, CancellationToken cancellationToken)
    {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        NpgsqlCommand command = dataSource.CreateCommand(
            $"CREATE PUBLICATION {publicationName} FOR TABLE {tableName}");

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> PublicationExists(string publicationName, CancellationToken cancellationToken)
    {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        NpgsqlCommand command = dataSource.CreateCommand(
            "SELECT EXISTS(SELECT 1 FROM pg_publication WHERE pubname = $1)");
        command.Parameters.AddWithValue(publicationName);

        return ((await command.ExecuteScalarAsync(cancellationToken)) as bool?) == true;
    }

    public async Task CreateReplicationSlot(string replicationSlotName, CancellationToken cancellationToken)
    {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        NpgsqlCommand command = dataSource.CreateCommand(
            "SELECT * FROM pg_create_logical_replication_slot($1, 'pgoutput');");
        command.Parameters.AddWithValue(replicationSlotName);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ReplicationSlotExists(string replicationSlotName, CancellationToken cancellationToken)
    {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        NpgsqlCommand command = dataSource.CreateCommand(
            "SELECT EXISTS(SELECT 1 FROM pg_replication_slots WHERE slot_name = $1 AND slot_type = 'logical')");
        command.Parameters.AddWithValue(replicationSlotName);

        return ((await command.ExecuteScalarAsync(cancellationToken)) as bool?) == true;
    }
}
