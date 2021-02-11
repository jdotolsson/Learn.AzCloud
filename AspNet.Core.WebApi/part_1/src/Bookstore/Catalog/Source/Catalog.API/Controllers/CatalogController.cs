using System.Collections.Generic;
using System.Linq;
using Catalog.API.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Catalog.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[ApiVersion(ApiVersionName.V1)]
	[SwaggerResponse(StatusCodes.Status500InternalServerError)]
	public class CatalogController : ControllerBase
	{
		private readonly ILogger<CatalogController> _logger;

		public CatalogController(ILogger<CatalogController> logger)
		{
			_logger = logger;
		}

		private static readonly IEnumerable<dynamic> Products = new List<dynamic>
		{
			new {Id = 1, Name = "Fish Without Hate", Author = "HACHIRO PERALEZ", Description = "A book about fish and people that hates fish, but not the other way around.", Price= 15m},
			new {Id = 2, Name = "Wife Of The North", Author = "LAURENCE COLON", Description = "The main story is about the wife of the famous man called the North.", Price= 666m},
			new {Id = 3, Name = "Men Of The East", Author = "KRIS CHAMBERS", Description = "Kris Chambers describes his life in this biography.", Price= 4.99m},
			new {Id = 4, Name = "Women Of Tomorrow", Author = "JOE ATKINSON", Description = "Women of tomorrow is the story about empowering the ongoing feminism movement in a modern perspective.", Price= 14.99m},
			new {Id = 5, Name = "Turtles And Spiders", Author = "TYLER MITCHELL", Description = "What does turtles and spiders in common? Read this book and you will be surprised.", Price= 39m}
		};

		/// <summary>
		/// Returns all the products in the catalog
		/// </summary>
		/// <returns>A 200 OK response.</returns>
		[HttpGet(Name = "[controller]" + nameof(GetProducts))]
		[SwaggerResponse(StatusCodes.Status200OK, "Gets all the products")]
		public IActionResult GetProducts()
		{
			_logger.LogInformation("My Custom Log");
			return Ok(Products);
		}

		/// <summary>
		/// Returns a specific product in the catalog
		/// </summary>
		/// <returns>A 200 OK response.</returns>
		/// <returns>A 404 Not Found response.</returns>
		[HttpGet("{bookId:int}",Name = "[controller]" + nameof(GetProduct))]
		[SwaggerResponse(StatusCodes.Status200OK, "Get a specific product with the provided bookId")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "Could not find the book with the provided bookId", typeof(ProblemDetails))]
		public IActionResult GetProduct(int bookId)
		{
			var book = Products.FirstOrDefault(x => x.Id == bookId);
			if (book == null)
				return NotFound();
			return Ok(book);
		}
	}
}
