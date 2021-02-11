using Catalog.API.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Catalog.API.Features.HealthChecks
{
	public static class ApplicationBuilderExtensions
	{
		public static void UseHealthChecks(this IApplicationBuilder app)
		{
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHealthChecks("/api/health/live", new HealthCheckOptions
				{
					Predicate = check => false
				}).RequireCors(CorsPolicyName.AllowAny)
				.AllowAnonymous();

				endpoints.MapHealthChecks("/api/health/ready", new HealthCheckOptions
				{
					Predicate = check => true //TODO include ready tags
				}).RequireCors(CorsPolicyName.AllowAny)
				.AllowAnonymous();
			});
		}
	}
}