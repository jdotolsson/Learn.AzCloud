using System;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Catalog.API.Settings;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Catalog.API.Utilities
{
	public static class ContainerBuilderExtensions
	{
		/// <summary>
		/// Registers <see cref="IOptions{TOptions}"/> and <typeparamref name="TOptions"/> to the services container.
		/// Also runs data annotation validation.
		/// </summary>
		/// <typeparam name="TOptions">The type of the options.</typeparam>
		/// <param name="services">The services collection.</param>
		/// <param name="configuration">The configuration.</param>
		/// <returns>The same services collection.</returns>
		public static IServiceCollection ConfigureAndValidateSingleton<TOptions>(
			this IServiceCollection services,
			IConfiguration configuration)
			where TOptions : class, new()
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			services
				.AddOptions<TOptions>()
				.Bind(configuration)
				.ValidateDataAnnotations();
			return services.AddSingleton(x => x.GetRequiredService<IOptions<TOptions>>().Value);
		}

		/// <summary>
		/// Adds dynamic response compression to enable GZIP compression of responses. This is turned off for HTTPS
		/// requests by default to avoid the BREACH security vulnerability.
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="configuration">The configuration.</param>
		/// <returns>The services with response compression services added.</returns>
		public static IServiceCollection AddCustomResponseCompression(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			return services
				.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal)
				.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal)
				.AddResponseCompression(
					options =>
					{
						// Add additional MIME types (other than the built in defaults) to enable GZIP compression for.
						var customMimeTypes = configuration
							.GetSection(nameof(AppSettings.Compression))
							.Get<CompressionOptions>()
							?.MimeTypes ?? Enumerable.Empty<string>();
						options.MimeTypes = customMimeTypes.Concat(ResponseCompressionDefaults.MimeTypes);

						options.Providers.Add<BrotliCompressionProvider>();
						options.Providers.Add<GzipCompressionProvider>();
					});
		}

		public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
		{
			return services
				.AddApiVersioning(
					options =>
					{
						options.AssumeDefaultVersionWhenUnspecified = true;
						options.ReportApiVersions = true;
						options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
						options.DefaultApiVersion = new ApiVersion(1, 0);
					})
				.AddVersionedApiExplorer(x => x.GroupNameFormat = "'v'VVV");
		}


		public static IServiceCollection AddCustomProblemDetails(
			this IServiceCollection services,
			IWebHostEnvironment webHostEnvironment)
		{
			return services.AddProblemDetails(options =>
			{
				// Only include exception details in a development environment. There's really no nee
				// to set this as it's the default behavior. It's just included here for completeness :)
				options.IncludeExceptionDetails = (ctx, ex) => webHostEnvironment.IsDevelopment();

				// You can configure the middleware to re-throw certain types of exceptions, all exceptions or based on a predicate.
				// This is useful if you have upstream middleware that needs to do additional handling of exceptions.
				options.Rethrow<NotSupportedException>();

				// This will map NotImplementedException to the 501 Not Implemented status code.
				options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

				// This will map HttpRequestException to the 503 Service Unavailable status code.
				options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

				// Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
				// If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
				options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
			});
		}

		public static IMvcBuilder AddCustomJsonOptions(
			this IMvcBuilder builder,
			IWebHostEnvironment webHostEnvironment)
		{
			return builder.AddJsonOptions(
				options =>
				{
					var jsonSerializerOptions = options.JsonSerializerOptions;
					if (webHostEnvironment.IsDevelopment())
					{
						// Pretty print the JSON in development for easier debugging.
						jsonSerializerOptions.WriteIndented = true;
					}

					jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
					jsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
					jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				});
		}

		public static IMvcBuilder AddCustomMvcOptions(this IMvcBuilder builder)
		{
			return builder.AddMvcOptions(
				options =>
				{
					 // Remove plain text (text/plain) output formatter.
					 options.OutputFormatters.RemoveType<StringOutputFormatter>();

					 // Add support for JSON Patch (application/json-patch+json) by adding an input formatter.
					 options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());

					var jsonInputFormatterMediaTypes = options
						.InputFormatters
						.OfType<SystemTextJsonInputFormatter>()
						.First()
						.SupportedMediaTypes;
					var jsonOutputFormatterMediaTypes = options
						.OutputFormatters
						.OfType<SystemTextJsonOutputFormatter>()
						.First()
						.SupportedMediaTypes;

					 // Remove JSON text (text/json) media type from the JSON input and output formatters.
					 jsonInputFormatterMediaTypes.Remove("text/json");
					jsonOutputFormatterMediaTypes.Remove("text/json");

					 // Add ProblemDetails media type (application/problem+json) to the output formatters.
					 // See https://tools.ietf.org/html/rfc7807
					 jsonOutputFormatterMediaTypes.Insert(0, "application/problem+json");

					 // Add RESTful JSON media type (application/vnd.restful+json) to the JSON input and output formatters.
					 // See http://restfuljson.org/
					 jsonInputFormatterMediaTypes.Insert(0, "application/vnd.restful+json");
					jsonOutputFormatterMediaTypes.Insert(0, "application/vnd.restful+json");

					 // Returns a 406 Not Acceptable if the MIME type in the Accept HTTP header is not valid.
					 options.ReturnHttpNotAcceptable = true;
				});
		}

		/// <summary>
		/// Gets the JSON patch input formatter. The <see cref="JsonPatchDocument"/> does not support the new
		/// System.Text.Json API's for de-serialization. You must use Newtonsoft.Json instead (See
		/// https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-3.0#jsonpatch-addnewtonsoftjson-and-systemtextjson).
		/// </summary>
		/// <returns>The JSON patch input formatter using Newtonsoft.Json.</returns>
		private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
		{
			var services = new ServiceCollection()
				.AddLogging()
				.AddMvc()
				.AddNewtonsoftJson()
				.Services;
			var serviceProvider = services.BuildServiceProvider();
			var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value;
			return mvcOptions.InputFormatters
				.OfType<NewtonsoftJsonPatchInputFormatter>()
				.First();
		}
	}
}