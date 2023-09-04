using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace EF.Postgres.LogicalReplication.UnitTests;

public class LogicalReplicationListenerTests : IAsyncLifetime
{
    private readonly LogicalReplicationListener<AppDbContext, Book> _sut;
    private readonly AppDbContext _dbContext;
    private readonly IInsertHandler<Book> _insertHandler;

    public LogicalReplicationListenerTests()
    {
        _dbContext = new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=BooksStore;Username=postgres;Password=postgres").Options);

        _insertHandler = Substitute.For<IInsertHandler<Book>>();

        _sut = new LogicalReplicationListener<AppDbContext, Book>(_dbContext, _insertHandler);
    }

    public async Task InitializeAsync()
    {
        await _sut.StartAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await _sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task InsertingNewItemShouldBeObserved()
    {
        // Arrange
        Book book = new() { Id = 1, Title = "Boom 1" };

        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();

        // Act
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        //Assert
        await Wait(() => !_insertHandler.ReceivedCalls().Any(), TimeSpan.FromSeconds(1));

        await _insertHandler.Received().Handle(Arg.Is<Book>(b => b.Id == book.Id && b.Title == book.Title), Arg.Any<CancellationToken>());
    }

    private static async Task Wait(Func<bool> waitWhile, TimeSpan waitTimeOut)
    {
        const int delay = 500;

        int waitCounter = 0;
        int waitLimit = (int)(waitTimeOut.TotalMilliseconds / delay);

        while (waitWhile() && waitCounter < waitLimit)
        {
            await Task.Delay(delay);
        }
    }
}