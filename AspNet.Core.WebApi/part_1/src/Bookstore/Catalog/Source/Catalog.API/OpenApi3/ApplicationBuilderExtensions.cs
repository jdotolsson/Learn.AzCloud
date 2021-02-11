using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.OpenApi3
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder application)
		{
			return application.UseSwaggerUI(
				options =>
				{
					// Set the Swagger UI browser document title.
					options.DocumentTitle = typeof(Startup)
						.Assembly
						.GetCustomAttribute<AssemblyProductAttribute>()
						.Product;
					// Set the Swagger UI to render at '/'.
					options.RoutePrefix = string.Empty;

					options.DisplayOperationId();
					options.DisplayRequestDuration();

					var provider = application.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
					foreach (var apiVersionDescription in provider
						.ApiVersionDescriptions
						.OrderByDescending(x => x.ApiVersion))
					{
						options.SwaggerEndpoint(
							$"/swagger/{apiVersionDescription.GroupName}/swagger.json",
							$"Version {apiVersionDescription.ApiVersion}");
					}
				});
		}
	}
}