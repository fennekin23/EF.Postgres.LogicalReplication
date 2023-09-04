namespace EF.Postgres.LogicalReplication;

internal class DefaultNamingConventions(string tableName) : INamingConventions
{
    private readonly string _publication = $"{tableName.ToLowerInvariant()}_publication";
    private readonly string _slot = $"{tableName.ToLowerInvariant()}_slot";

    public string GetPublicationName() => _publication;

    public string GetSlotName() => _slot;
}
