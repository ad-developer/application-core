using ApplicationCore.Rules;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services;

public class TestService : BaseService<TestService>
{
    public TestService(IRulePipeline rulePipeline, ILogger<TestService> logger)
        : base(rulePipeline, logger)
    {
    }
}
