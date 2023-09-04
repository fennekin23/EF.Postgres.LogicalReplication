using EF.Postgres.LogicalReplication;
using EF.Postgres.LogicalReplication.Example.Handlers;
using EF.Postgres.LogicalReplication.Example.Storage;

using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostBuilder, services) =>
    {
        services
            .AddDbContext<AppDbContext>(o =>
                o.UseNpgsql(hostBuilder.Configuration.GetConnectionString("AppContext")));
        services
            .AddLogicalReplicationListener<AppDbContext, Book>()
            .AddSingleton<IInsertHandler<Book>, InsertBookHandler>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .UseConsoleLifetime()
    .Build();

host.Run();
