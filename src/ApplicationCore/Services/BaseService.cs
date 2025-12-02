using ApplicationCore.Data;
using ApplicationCore.DataPersistence;
using ApplicationCore.Rules.Abstractions;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services;

public abstract class BaseService<T>: IService
{
    public IRulePipeline RulePipeline { get; }
    public Guid InstanceId => RulePipeline.InstanceId;
    public ILogger Logger { get; }
    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        RulePipeline = rulePipeline;
        Logger = logger;
        Logger.LogInformation($"{GetType().Name} initialized.");
    }
}

public abstract class BaseService<T, R1> : IService<R1>
{
    public IRulePipeline RulePipeline { get; }
    public Guid InstanceId => RulePipeline.InstanceId;
    public R1 RepositoryOne { get; }
    public ILogger Logger { get; }

    public BaseService(IRulePipeline rulePipeline,  ILogger<T> logger, R1 repositoryOne)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(repositoryOne, nameof(repositoryOne));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        RulePipeline = rulePipeline;
        RepositoryOne = repositoryOne;
        Logger = logger;
        
        Logger.LogInformation($"{GetType().Name} initialized.");
    }
}

public abstract class BaseService<T, R1, R2> : IService<R1, R2>, IUnitOfWork
{
    public IRulePipeline RulePipeline { get; }
    public R1 RepositoryOne { get; }
    public R2 RepositoryTwo { get; }
    public IContext Context {get;}
    public ILogger Logger { get; }

    public Guid InstanceId => Guid.NewGuid(); // Assuming each service instance has a unique ID

    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger, R1 repositoryOne, R2 repositoryTwo, IContext context)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(repositoryOne, nameof(repositoryOne));
        ArgumentNullException.ThrowIfNull(repositoryTwo, nameof(repositoryTwo));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        RulePipeline = rulePipeline;
        
        Context = context;

        RepositoryOne = repositoryOne;
        (RepositoryOne as IBaseRepository).Context = context;

        RepositoryTwo = repositoryTwo;
        (RepositoryTwo as IBaseRepository).Context = context;

        Logger = logger;

        Logger.LogInformation($"{GetType().Name} initialized.");
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