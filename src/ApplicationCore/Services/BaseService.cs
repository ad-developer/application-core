using ApplicationCore.DataPersistence;
using ApplicationCore.Logging;
using ApplicationCore.Rules;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services;

public abstract class BaseService<T> : IService
{
    public IRulePipeline RulePipeline { get; }
    public ILogger Logger { get; }
    public Guid InstanceId => RulePipeline.InstanceId;

    public Guid TrackingId { get; set ; } = Guid.NewGuid();
    public LoggerIdentity LoggerIdentity { get; set; }

    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger, ILoggerIdentityService loggerIdentityService)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(loggerIdentityService, nameof(loggerIdentityService));

        RulePipeline = rulePipeline;
        Logger = logger;

        LoggerIdentity = loggerIdentityService.GetLoggerIdentity();

        this.LogInformation($"{GetType().Name} initialized.");
    }
}

public abstract class BaseService<T, R1> : IService<R1>
{
    public IRulePipeline RulePipeline { get; }
    public ILogger Logger { get; }
    public Guid InstanceId => RulePipeline.InstanceId;
    public R1 RepositoryOne { get; }
    public Guid TrackingId { get; set ; } = Guid.NewGuid();
    public LoggerIdentity LoggerIdentity { get; set; }


    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger, ILoggerIdentityService loggerIdentityService, R1 repositoryOne)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(repositoryOne, nameof(repositoryOne));
        ArgumentNullException.ThrowIfNull(loggerIdentityService, nameof(loggerIdentityService));

        RulePipeline = rulePipeline;
        Logger = logger;
        RepositoryOne = repositoryOne;

        LoggerIdentity = loggerIdentityService.GetLoggerIdentity();

        this.LogInformation($"{GetType().Name} initialized.");
    }
}

public abstract class BaseService<T, R1, R2> : IService<R1, R2>, IUnitOfWork
{
    public IRulePipeline RulePipeline { get; }
    public ILogger Logger { get; }
    public Guid InstanceId => RulePipeline.InstanceId;
    public R1 RepositoryOne { get; }
    public R2 RepositoryTwo { get; }
    public IContext Context {get;}
    public Guid TrackingId { get; set ; } = Guid.NewGuid();
    public LoggerIdentity LoggerIdentity { get; set; }

    public BaseService(IRulePipeline rulePipeline, ILogger<T> logger, ILoggerIdentityService loggerIdentityService, R1 repositoryOne, R2 repositoryTwo, IContext context)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(repositoryOne, nameof(repositoryOne));
        ArgumentNullException.ThrowIfNull(repositoryTwo, nameof(repositoryTwo));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(loggerIdentityService, nameof(loggerIdentityService));

        RulePipeline = rulePipeline;
        Logger = logger;
        Context = context;

        RepositoryOne = repositoryOne;
        (RepositoryOne as IBaseRepository).Context = context;

        RepositoryTwo = repositoryTwo;
        (RepositoryTwo as IBaseRepository).Context = context;

        LoggerIdentity = loggerIdentityService.GetLoggerIdentity();

        this.LogInformation($"{GetType().Name} initialized.");
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