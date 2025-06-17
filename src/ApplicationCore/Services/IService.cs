using ApplicationCore.DataPersistence;
using ApplicationCore.Rules;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services;

public interface IService<T>
{
    IRulePipeline RulePipeline { get; }
    ILogger<T> Logger { get; }
}

public interface IService<T, R1> : IService<T> 
{
    R1 RepositoryOne { get; }
}

public interface IService<T, R1, R2> : IService<T>
{
    IContext Context { get; }
    R1 RepositoryOne { get; }
    R2 RepositoryTwo { get; }
}