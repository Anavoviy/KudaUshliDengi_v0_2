using KudaUshliDengi_v0_2.domain.events.interfaces;
using KudaUshliDengi_v0_2.domain.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.infrastructure.ef_core.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KudaUshliDengi_v0_2.infrastructure.ef_core.context;

public class SqliteDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainDispatcher _domainDispatcher;
    private IDbContextTransaction _transaction;
    
    public DbSet<User> Users { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Operation> Operations { get; set; }

    public SqliteDbContext(IDomainDispatcher domainDispatcher, DbContextOptions<SqliteDbContext> options) : base(options)
    {
        _domainDispatcher = domainDispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new GoalConfiguration());
        modelBuilder.ApplyConfiguration(new OperationConfiguration());
    }


    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<Entity<EntityId<Guid>>>()
            .SelectMany(entry => entry.Entity.PopDomainEvents())
            .ToList();

        var result = await SaveChangesAsync(cancellationToken);
        
        if(domainEvents.Any())
            await _domainDispatcher.DispatchAsync(domainEvents, cancellationToken);
        
        return result;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
        => await RollbackTransactionAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) return;
        _transaction = await Database.BeginTransactionAsync(cancellationToken);
    }
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) return;

        try
        {
            await CommitAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) return;
        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
            ChangeTracker.Clear();
        }
    }
}