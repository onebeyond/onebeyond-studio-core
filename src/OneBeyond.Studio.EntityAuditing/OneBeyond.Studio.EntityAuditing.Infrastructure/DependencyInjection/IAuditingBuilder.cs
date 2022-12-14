using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure.DependencyInjection;

public interface IAuditingBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}
