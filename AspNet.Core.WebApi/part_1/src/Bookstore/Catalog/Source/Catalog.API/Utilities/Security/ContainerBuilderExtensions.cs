using System;
using Catalog.API.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.Utilities.Security
{
	public static class ContainerBuilderExtensions
	{
		public static IServiceCollection AddCustomCors(this IServiceCollection services)
		{
			return services.AddCors(
				options =>
					// Create named CORS policies here which you can consume using application.UseCors("PolicyName")
					// or a [EnableCors("PolicyName")] attribute on your controller or action.
					options.AddPolicy(
						CorsPolicyName.AllowAny,
						x => x
							.AllowAnyOrigin()
							.AllowAnyMethod()
							.AllowAnyHeader()));
		}

		public static IServiceCollection AddCustomStrictTransportSecurity(this IServiceCollection services)
		{
			return services
				.AddHsts(
				options =>
				{
					// Preload the HSTS HTTP header for better security. See https://hstspreload.org/
					options.IncludeSubDomains = true;
					options.MaxAge = TimeSpan.FromSeconds(31536000); // 1 Year
					options.Preload = true;
				});
		}
	}
}