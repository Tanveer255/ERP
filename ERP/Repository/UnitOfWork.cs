using ERP.Data;

namespace ERP.Repository;

/// <summary>
/// Unit Of Work interface.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets context property.
    /// </summary>
    ManufacturingDbContext Context { get; }

    /// <summary>
    /// Commits all changes made in the current unit of work to the database asynchronously.
    /// </summary>
    Task CommitAsync();
}

/// <summary>
/// Class of UnitOfWork.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ILogger<UnitOfWork> _logger;

    /// <summary>
    /// Gets this is DBContext class.
    /// </summary>
    public ManufacturingDbContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">object of ManufacturingDbContext.</param>
    /// <param name="logger">object of ILogger.</param>
    public UnitOfWork(ManufacturingDbContext context, ILogger<UnitOfWork> logger)
    {
        Context = context;
        _logger = logger;
    }

    /// <summary>
    /// Commit save changes.
    /// </summary>
    public async Task CommitAsync()
    {
        try
        {
            _logger.LogInformation("Commit save changes");
            await Context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, ex.InnerException, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            throw new Exception(ex.Message, ex.InnerException);
        }
    }

    /// <summary>
    /// Dispose method.
    /// </summary>
    public void Dispose()
    {
        Context.Dispose();
    }
}
