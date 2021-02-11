using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Catalog.API.OpenApi3
{
	public static class ContainerBuilderExtensions
	{

		/// <summary>
		/// Adds Swagger services and configures the Swagger services.
		/// </summary>
		/// <param name="services">The services.</param>
		/// <returns>The services with Swagger services added.</returns>
		public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
		 {
			 return services.AddSwaggerGen(
				 options =>
				 {
					 var assembly = typeof(Startup).Assembly;
					 var assemblyProduct = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
					 var assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

					 options.DescribeAllParametersInCamelCase();
					 options.EnableAnnotations();

					 // Add the XML comment file for this assembly, so its contents can be displayed.
					 options.IncludeXmlCommentsIfExists(assembly);

					 options.OperationFilter<ApiVersionOperationFilter>();
					 options.OperationFilter<ClaimsOperationFilter>();
					 options.OperationFilter<ForbiddenResponseOperationFilter>();
					 options.OperationFilter<UnauthorizedResponseOperationFilter>();

					 // Show a default and example model for JsonPatchDocument<T>.
					 options.SchemaFilter<JsonPatchDocumentSchemaFilter>();

					 var provider = services.BuildServiceProvider()
						 .GetRequiredService<IApiVersionDescriptionProvider>();
					 foreach (var apiVersionDescription in provider.ApiVersionDescriptions)
					 {
						 var info = new OpenApiInfo()
						 {
							 Title = assemblyProduct,
							 Description = apiVersionDescription.IsDeprecated
								 ? $"{assemblyDescription} This API version has been deprecated."
								 : assemblyDescription,
							 Version = apiVersionDescription.ApiVersion.ToString(),
						 };
						 options.SwaggerDoc(apiVersionDescription.GroupName, info);
					 }
				 });
		 }
	}
}