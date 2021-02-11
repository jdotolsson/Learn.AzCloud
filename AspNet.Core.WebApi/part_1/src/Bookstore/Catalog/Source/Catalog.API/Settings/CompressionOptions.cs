using System.Collections.Generic;

namespace Catalog.API.Settings
{
	public class CompressionOptions
	{
		public CompressionOptions() => MimeTypes = new List<string>();

		
		public List<string> MimeTypes { get; }
	}
}