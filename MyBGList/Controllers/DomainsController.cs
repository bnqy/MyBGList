using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.DTO;
using MyBGList.Models;
using System.Diagnostics;
using MyBGList.Attributes;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using MyBGList.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace MyBGList.Controllers;

[Route("[controller]")]
[ApiController]
public class DomainsController : ControllerBase
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<DomainsController> _logger;

	public DomainsController(ApplicationDbContext context, ILogger<DomainsController> logger)
	{
		_context = context;
		_logger = logger;
	}

	/// <summary>
	/// Gets a list of domains
	/// </summary>
	/// <remarks>Retrieves a list of domains with custom paging, sorting, and filtering rules</remarks>>
	/// <param name="input"> A DTO object that can be used to customize some retrieval parameters</param>
	/// <returns>A RestDTO object containing a list of domains</returns>
	[HttpGet(Name = "GetDomains")]
	//[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
	[ResponseCache(CacheProfileName = "Any-60")]
	[ManualValidationFilter]
	public async Task<ActionResult<RestDTO<Domain[]>>> Get([FromQuery] RequestDTO<DomainDTO> input)
	{
		if (!ModelState.IsValid)
		{
			var details = new ValidationProblemDetails(ModelState);
			details.Extensions["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

			if (ModelState.Keys.Any(k => k == "PageSize"))
			{
				details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2";
				details.Status = StatusCodes.Status501NotImplemented;
				return new ObjectResult(details)
				{
					StatusCode = StatusCodes.Status501NotImplemented
				};
			}
			else
			{
				details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
				details.Status = StatusCodes.Status400BadRequest;
				return new BadRequestObjectResult(details);
			}
		}

		var query = _context.Domains.AsQueryable();
		if (!string.IsNullOrEmpty(input.FilterQuery))
			query = query.Where(b => b.Name.Contains(input.FilterQuery));
		var recordCount = await query.CountAsync();
		query = query
			.OrderBy($"{input.SortColumn} {input.SortOrder}")
			.Skip(input.PageIndex * input.PageSize)
			.Take(input.PageSize);

		return new RestDTO<Domain[]>()
		{
			Data = await query.ToArrayAsync(),
			PageIndex = input.PageIndex,
			PageSize = input.PageSize,
			RecordCount = recordCount,
			Links = new List<LinkDTO> {
				new LinkDTO(
					Url.Action(
						null,
						"Domains",
						new { input.PageIndex, input.PageSize },
						Request.Scheme)!,
					"self",
					"GET"),
			}
		};
	}

	[Authorize(Roles = RoleNames.Moderator)]
	[ApiExplorerSettings(IgnoreApi = true)]
	[HttpPost(Name = "UpdateDomain")]
	//[ResponseCache(NoStore = true)]
	[ResponseCache(CacheProfileName = "NoCache")]
	[SwaggerOperation(Summary = "Updates domain data", Description = "Updates domain table in DB")]
	public async Task<RestDTO<Domain?>> Post(
		[SwaggerParameter("DTO object contains domain data")]
		DomainDTO model)
	{
		var domain = await _context.Domains
			.Where(b => b.Id == model.Id)
			.FirstOrDefaultAsync();
		if (domain != null)
		{
			if (!string.IsNullOrEmpty(model.Name))
				domain.Name = model.Name;
			domain.LastModifiedDate = DateTime.Now;
			_context.Domains.Update(domain);
			await _context.SaveChangesAsync();
		};

		return new RestDTO<Domain?>()
		{
			Data = domain,
			Links = new List<LinkDTO>
				{
					new LinkDTO(
							Url.Action(
								null,
								"Domains",
								model,
								Request.Scheme)!,
							"self",
							"POST"),
				}
		};
	}

	[Authorize(Roles = RoleNames.Administrator)]
	[HttpDelete(Name = "DeleteDomain")]
	[ApiExplorerSettings(IgnoreApi = true)]
	//[ResponseCache(NoStore = true)]
	[ResponseCache(CacheProfileName = "NoCache")]
	[SwaggerOperation(Summary = "Deletes domain data", Description = "Deletes domain data in DB by id")]
	public async Task<RestDTO<Domain?>> Delete([SwaggerParameter("id of domain data in DB")] int id)
	{
		var domain = await _context.Domains
			.Where(b => b.Id == id)
			.FirstOrDefaultAsync();
		if (domain != null)
		{
			_context.Domains.Remove(domain);
			await _context.SaveChangesAsync();
		};

		return new RestDTO<Domain?>()
		{
			Data = domain,
			Links = new List<LinkDTO>
				{
					new LinkDTO(
							Url.Action(
								null,
								"Domains",
								id,
								Request.Scheme)!,
							"self",
							"DELETE"),
				}
		};
	}
}
