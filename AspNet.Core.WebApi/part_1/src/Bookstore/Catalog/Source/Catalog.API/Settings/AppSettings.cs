using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;

namespace Catalog.API.Settings
{
	public class AppSettings
	{ 
		[Required]
		public CompressionOptions Compression { get; set; }

		[Required]
		public ForwardedHeadersOptions ForwardedHeaders { get; set; }
	}
}
