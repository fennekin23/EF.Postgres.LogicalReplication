namespace EF.Postgres.LogicalReplication;

public class LogicalReplicationListenerOptions
{
    public string? ConnectionString { get; set; } = null;

    public bool CreateDatabasePublication { get; set; } = false;
}
