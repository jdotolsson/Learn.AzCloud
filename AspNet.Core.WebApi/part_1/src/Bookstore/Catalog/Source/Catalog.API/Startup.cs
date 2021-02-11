using Catalog.API.Constants;
using Catalog.API.Features.HealthChecks;
using Catalog.API.OpenApi3;
using Catalog.API.Settings;
using Catalog.API.Utilities;
using Catalog.API.Utilities.Security;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Catalog.API
{
	public class Startup
	{
		private readonly IConfiguration _configuration;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
		{
			_configuration = configuration;
			_webHostEnvironment = webHostEnvironment;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			Log.Logger.Information("Configuring Services");
			services.AddApplicationInsightsTelemetry(_configuration);
			services.AddAppSettings(_configuration);
			services.AddCustomCors();
			services.AddRouting(options => options.LowercaseUrls = true);
			services.AddResponseCaching();
			services.AddCustomResponseCompression(_configuration);
			services.AddCustomStrictTransportSecurity();
			services.AddCustomHealthChecks();
			services.AddCustomSwagger();
			services.AddHttpContextAccessor();
			// Add useful interface for accessing the ActionContext outside a controller.
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
			services.AddCustomApiVersioning();
			services.AddControllers()
				.AddCustomJsonOptions(_webHostEnvironment)
				.AddCustomMvcOptions();
			services.AddCustomProblemDetails(_webHostEnvironment);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}
			
			app.UseProblemDetails();
			app.UseForwardedHeaders();
			app.UseResponseCaching();
			app.UseResponseCompression();
			app.UseRouting();
			app.UseCors(CorsPolicyName.AllowAny);
			app.UseCustomSerilogRequestLogging();
			app.UseHealthChecks();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers().RequireCors(CorsPolicyName.AllowAny);
			});
			app.UseSwagger();
			app.UseCustomSwaggerUI();
		}
	}
}
