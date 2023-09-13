namespace EF.Postgres.LogicalReplication;

internal class DefaultNamingConventions(string prefix) : INamingConventions
{
    private readonly string _publication = $"{prefix.ToLowerInvariant()}_publication";
    private readonly string _slot = $"{prefix.ToLowerInvariant()}_slot";

    public string GetPublicationName() => _publication;

    public string GetSlotName() => _slot;
}
