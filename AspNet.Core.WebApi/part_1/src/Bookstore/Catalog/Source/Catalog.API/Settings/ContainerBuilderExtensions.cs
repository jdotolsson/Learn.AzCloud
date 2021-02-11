using Catalog.API.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.Settings
{
	public static class ContainerBuilderExtensions
	{
		public static IServiceCollection AddAppSettings(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			return services
				// ConfigureAndValidateSingleton registers IOptions<T> and also T as a singleton to the services collection.
				.ConfigureAndValidateSingleton<AppSettings>(configuration)
				.ConfigureAndValidateSingleton<CompressionOptions>(
					configuration.GetSection(nameof(AppSettings.Compression)))
				.ConfigureAndValidateSingleton<ForwardedHeadersOptions>(
					configuration.GetSection(nameof(AppSettings.ForwardedHeaders)))
				.Configure<ForwardedHeadersOptions>(
					options =>
					{
						options.KnownNetworks.Clear();
						options.KnownProxies.Clear();
					});
		}
	}
}