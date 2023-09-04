namespace EF.Postgres.LogicalReplication;

public interface INamingConventions
{
    string GetPublicationName();

    string GetSlotName();
}
