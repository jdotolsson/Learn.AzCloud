using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.Features.HealthChecks
{
	public static class ContainerBuilderExtensions
	{
		public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
		{
			return services
				.AddHealthChecks()
				.Services;
		}
	}
}
