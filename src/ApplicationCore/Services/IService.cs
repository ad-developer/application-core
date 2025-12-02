using ApplicationCore.DataPersistence;
using ApplicationCore.Rules.Abstractions;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services;

public interface IService  
{
    IRulePipeline RulePipeline { get; }
    Guid InstanceId { get; }
    ILogger Logger { get; }
}

public interface IService<R1> : IService 
{
    R1 RepositoryOne { get; }
}

public interface IService<R1, R2> : IService
{
    IContext Context { get; }
    R1 RepositoryOne { get; }
    R2 RepositoryTwo { get; }
}