using ApplicationCore.DataPersistence;
using ApplicationCore.Rules;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services;

public abstract class BaseService<T> : IService<T>
{
    public IRulePipeline RulePipeline { get; }
    public ILogger<T> Logger { get; }
    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        RulePipeline = rulePipeline;
        Logger = logger;
    }
}

public abstract class BaseService<T, R1> : IService<T, R1>
{
    public IRulePipeline RulePipeline { get; }
    public ILogger<T> Logger { get; }
    public R1 RepositoryOne { get; }

    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger, R1 repositoryOne)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(repositoryOne, nameof(repositoryOne));

        RulePipeline = rulePipeline;
        Logger = logger;
        RepositoryOne = repositoryOne;
    }
}

public abstract class BaseService<T, R1, R2> : IService<T, R1, R2>, IUnitOfWork
{
    public IRulePipeline RulePipeline { get; }
    public ILogger<T> Logger { get; }
    public R1 RepositoryOne { get; }
    public R2 RepositoryTwo { get; }

    public IContext Context {get;}

    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger, R1 repositoryOne, R2 repositoryTwo, IContext context)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(repositoryOne, nameof(repositoryOne));
        ArgumentNullException.ThrowIfNull(repositoryTwo, nameof(repositoryTwo));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        RulePipeline = rulePipeline;
        Logger = logger;
        Context = context;

        RepositoryOne = repositoryOne;
        (RepositoryOne as IBaseRepository).Context = context;

        RepositoryTwo = repositoryTwo;
        (RepositoryTwo as IBaseRepository).Context = context;
        
        Logger.LogInformation($"{GetType().Name} initialized with repositories and context.");
    }

    public void SaveChanges()
    {
       Context.SaveChanges();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Context.SaveChangesAsync(cancellationToken);
    }
}