using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Catalog.API.Utilities;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace Catalog.API
{
	public class Program
	{
		public static Task<int> Main(string[] args) => LogAndRunAsync(CreateHostBuilder(args).Build());

		public static async Task<int> LogAndRunAsync(IHost host)
		{
			if (host is null)
			{
				throw new ArgumentNullException(nameof(host));
			}

			var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
			hostEnvironment.ApplicationName = Assembly
				.GetExecutingAssembly()
				.GetCustomAttribute<AssemblyProductAttribute>()?.Product;

			Log.Logger = CreateLogger(host);

			try
			{
				Log.Information(
					"Started {Application} in {Environment} mode.",
					hostEnvironment.ApplicationName,
					hostEnvironment.EnvironmentName);
				await host.RunAsync().ConfigureAwait(false);
				Log.Information(
					"Stopped {Application} in {Environment} mode.",
					hostEnvironment.ApplicationName,
					hostEnvironment.EnvironmentName);
				return 0;
			}
			catch (Exception exception)
			{
				Log.Fatal(
					exception,
					"{Application} terminated unexpectedly in {Environment} mode.",
					hostEnvironment.ApplicationName,
					hostEnvironment.EnvironmentName);
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
		
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureHostConfiguration(builder => builder
					.AddEnvironmentVariables("DOTNET_")
					.AddIf(args != null, x => x.AddCommandLine(args)))
				.ConfigureAppConfiguration((context, builder) => 
					AddConfiguration(builder, context.HostingEnvironment, args))
				.UseSerilog()
				.UseDefaultServiceProvider((context, options) =>
				{
					var isDevelopment = context.HostingEnvironment.IsDevelopment();
					options.ValidateScopes = isDevelopment;
					options.ValidateOnBuild = isDevelopment;
				})
				.ConfigureWebHost(ConfigureWebHostBuilder)
				.UseConsoleLifetime();


		private static void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder) =>
			webHostBuilder
				.UseKestrel(
					(builderContext, options) =>
					{
						options.AddServerHeader = false;
					})
				.UseAzureAppServices()
				// Used for IIS and IIS Express for in-process hosting. Use UseIISIntegration for out-of-process hosting.
				.UseIIS()
				.UseStartup<Startup>();

		private static Logger CreateLogger(IHost host)
		{
			var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
			return new LoggerConfiguration()
				.ReadFrom.Configuration(host.Services.GetRequiredService<IConfiguration>())
				.Enrich.WithProperty("Application", hostEnvironment.ApplicationName)
				.Enrich.WithProperty("Environment", hostEnvironment.EnvironmentName)
				.WriteTo.Conditional(
					x => hostEnvironment.IsDevelopment(),
					x => x.Console().WriteTo.Debug())
				.WriteTo.Conditional(
					x => hostEnvironment.IsProduction(),
					x => x.ApplicationInsights(
						host.Services.GetRequiredService<TelemetryConfiguration>(),
						TelemetryConverter.Traces))
				.CreateLogger();
		}

		 private static IConfigurationBuilder AddConfiguration(
			IConfigurationBuilder configurationBuilder,
			IHostEnvironment hostEnvironment,
			string[] args) =>
			configurationBuilder
				// Add configuration from the appsettings.json file.
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				// Add configuration from an optional appsettings.development.json, appsettings.staging.json or
				// appsettings.production.json file, depending on the environment. These settings override the ones in
				// the appsettings.json file.
				.AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
				// Add configuration from files in the specified directory. The name of the file is the key and the
				// contents the value.
				.AddKeyPerFile(Path.Combine(Directory.GetCurrentDirectory(), "configuration"), optional: true, reloadOnChange: false)
				// This reads the configuration keys from the secret store. This allows you to store connection strings
				// and other sensitive settings, so you don't have to check them into your source control provider.
				// Only use this in Development, it is not intended for Production use. See
				// http://docs.asp.net/en/latest/security/app-secrets.html
				.AddIf(
					hostEnvironment.IsDevelopment() && !string.IsNullOrEmpty(hostEnvironment.ApplicationName),
					x => x.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: false))
				// Add configuration specific to the Development, Staging or Production environments. This config can
				// be stored on the machine being deployed to or if you are using Azure, in the cloud. These settings
				// override the ones in all of the above config files. See
				// http://docs.asp.net/en/latest/security/app-secrets.html
				.AddEnvironmentVariables()
				// Push telemetry data through the Azure Application Insights pipeline faster in the development and
				// staging environments, allowing you to view results immediately.
				.AddApplicationInsightsSettings(developerMode: !hostEnvironment.IsProduction())
				// Add command line options. These take the highest priority.
				.AddIf(
					args is not null,
					x => x.AddCommandLine(args));
	}
}
