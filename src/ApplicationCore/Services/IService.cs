using ApplicationCore.DataPersistence;
using ApplicationCore.Logging;
using ApplicationCore.Rules;

namespace ApplicationCore.Services;

public interface IService  : ITrackable
{
    IRulePipeline RulePipeline { get; }
    Guid InstanceId { get; }
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