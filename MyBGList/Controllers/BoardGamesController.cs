using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBGList.DTO;
using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using MyBGList.Models;
using System.Linq.Dynamic.Core;
using System.ComponentModel.DataAnnotations;
using MyBGList.Attributes;
using MyBGList.Constants;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace MyBGList.Controllers;

//[Route("api/[controller]")]
[Route("[controller]")]
[ApiController]
public class BoardGamesController : ControllerBase
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<BoardGamesController> _logger;
	private readonly IMemoryCache _memoryCache;

	public BoardGamesController(ApplicationDbContext context,
		ILogger<BoardGamesController> logger,
		IMemoryCache memoryCache)
	{
		_context = context;
		_logger = logger;
		_memoryCache = memoryCache;
	}

	/*[HttpGet(Name = "GetBoardGames")]
	[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
	public async Task<RestDTO<BoardGame[]>> Get(int pageIndex = 0, 
		[Range(1, 100)]int pageSize = 10, 
		[SortColumnValidator(typeof(BoardGameDTO))] string? sortColumn = "Name",
		[SortOrderValidator] string? sortOrder = "ASC",
		string? filterQuery = null)
	{
		var query = _context.BoardGames.AsQueryable();
		if (!string.IsNullOrEmpty(filterQuery))
			query = query.Where(b => b.Name.Contains(filterQuery));

		var recordCount = await query.CountAsync();

		query = query
			.OrderBy($"{sortColumn} {sortOrder}")
			.Skip(pageIndex * pageSize)
			.Take(pageSize);

		return new RestDTO<BoardGame[]>() 
		{
			Data = await query.ToArrayAsync(),
			PageIndex = pageIndex,
			PageSize = pageSize,
			RecordCount = recordCount,
			Links = new List<LinkDTO> 
			{
				new LinkDTO(
					Url.Action(null, 
					"BoardGames", 
					new { pageIndex, pageSize }, 
					Request.Scheme)!,
					"self",
					"GET"), 
			}
		};
	}*/

	[HttpGet(Name = "GetBoardGames")]
	//[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
	[ResponseCache(CacheProfileName = "Any-60")]
	[SwaggerOperation(Summary = "Get a list of Board Games.",
		Description = "Retrieves a list of board games with custom paging, sorting, and filtering rules.")]
	public async Task<RestDTO<BoardGame[]>> Get([FromQuery] 
	[SwaggerParameter("A DTO object that can be used to customize some retrieval parameters.")] RequestDTO<BoardGameDTO> input)
	{
		_logger.LogInformation(CustomLogEvents.BoardGamesController_Get, "GET method started!");

		var query = _context.BoardGames.AsQueryable();
		if (!string.IsNullOrEmpty(input.FilterQuery))
			query = query.Where(b => b.Name.Contains(input.FilterQuery));

		var recordCount = await query.CountAsync();

		BoardGame[]? result = null;
		var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";

		if (!_memoryCache.TryGetValue<BoardGame[]>(cacheKey, out result))
		{
			query = query
				.OrderBy($"{input.SortColumn} {input.SortOrder}")
				.Skip(input.PageIndex * input.PageSize)
				.Take(input.PageSize);

			result = await query.ToArrayAsync();
			_memoryCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
		}

		return new RestDTO<BoardGame[]>()
		{
			Data = result,
			PageIndex = input.PageIndex,
			PageSize = input.PageSize,
			RecordCount = recordCount,
			Links = new List<LinkDTO>
			{
				new LinkDTO(
					Url.Action(null,
					"BoardGames",
					new { input.PageIndex, input.PageSize },
					Request.Scheme)!,
					"self",
					"GET"),
			}
		};
	}


	[Authorize(Roles = RoleNames.Moderator)]
	[HttpPost(Name = "UpdateBoardGame")]
	//[ResponseCache(NoStore = true)]
	[ResponseCache(CacheProfileName = "NoCache")]
	[SwaggerOperation(Summary = "Updates a board game data",
		Description = "Updates board game data in database")]
	public async Task<RestDTO<BoardGame?>> Post(BoardGameDTO model)
	{
		var boardgame = await _context.BoardGames
			.Where(b => b.Id == model.Id)
			.FirstOrDefaultAsync();

		if (boardgame != null)
		{
			if (!string.IsNullOrEmpty(model.Name))
				boardgame.Name = model.Name;
			if (model.Year.HasValue && model.Year.Value > 0)
				boardgame.Year = model.Year.Value;
			boardgame.LastModifiedDate = DateTime.Now;
			_context.BoardGames.Update(boardgame);
			await _context.SaveChangesAsync();
		};

		return new RestDTO<BoardGame?>()
		{
			Data = boardgame,
			Links = new List<LinkDTO>
				{
					new LinkDTO(
							Url.Action(
								null,
								"BoardGames",
								model,
								Request.Scheme)!,
							"self",
							"POST"),
				}
		};
	}

	[Authorize(Roles = RoleNames.Administrator)]
	[HttpDelete(Name = "DeleteBoardGame")]
	//[ResponseCache(NoStore = true)]
	[ResponseCache(CacheProfileName = "NoCache")]
	[SwaggerOperation(Summary = "Deletes a board game by id",
		Description = "Deletes a board game from database (id)")]
	public async Task<RestDTO<BoardGame?>> Delete(int id)
	{
		var boardgame = await _context.BoardGames
			.Where(b => b.Id == id)
			.FirstOrDefaultAsync();

		if (boardgame != null)
		{
			_context.BoardGames.Remove(boardgame);
			await _context.SaveChangesAsync();
		};

		return new RestDTO<BoardGame?>()
		{
			Data = boardgame,
			Links = new List<LinkDTO>
				{
					new LinkDTO(
							Url.Action(
								null,
								"BoardGames",
								id,
								Request.Scheme)!,
							"self",
							"DELETE"),
				}
		};
	}


	[HttpGet("{id}")]
	[ResponseCache(CacheProfileName = "Any-60")]
	[SwaggerOperation(Summary = "Get a board game by id",
		Description = "Retrieves a board game by given id")]
	public async Task<RestDTO<BoardGame?>> GetBoardGame([CustomKeyValue("x-test-3", "value 3")] int id)
	{
		_logger.LogInformation(CustomLogEvents.BoardGamesController_Get, "Get id of BoardGame started!");

		BoardGame? result = null;
		var cacheKey = $"GetBoardGame-{id}";

		if (!_memoryCache.TryGetValue<BoardGame>(cacheKey, out result))
		{
			result = await _context.BoardGames.FirstOrDefaultAsync(bg => bg.Id == id);
			_memoryCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
		}

		return new RestDTO<BoardGame?>()
		{
			Data = result,
			PageIndex = 0,
			PageSize = 1,
			RecordCount = result != null ? 1 : 0,
			Links = new List<LinkDTO>
			{
				new LinkDTO(Url.Action(null, "BoardGames", new {id}, Request.Scheme)!, 
				"self", 
				"GET")
			}
		};
	}
}
