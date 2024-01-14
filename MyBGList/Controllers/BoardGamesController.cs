using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBGList.DTO;
using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using MyBGList.Models;
using System.Linq.Dynamic.Core;

namespace MyBGList.Controllers;

//[Route("api/[controller]")]
[Route("[controller]")]
[ApiController]
public class BoardGamesController : ControllerBase
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<BoardGamesController> _logger;

	public BoardGamesController(ApplicationDbContext context,
		ILogger<BoardGamesController> logger)
	{
		_context = context;
		_logger = logger;
	}

	[HttpGet(Name = "GetBoardGames")]
	[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
	public async Task<RestDTO<BoardGame[]>> Get(int pageIndex = 0, 
		int pageSize = 10, 
		string? sortColumn = "Name",
		string? sortOrder = "ASC")
	{
		var query = _context.BoardGames
			.OrderBy($"{sortColumn} {sortOrder}")
			.Skip(pageIndex * pageSize)
			.Take(pageSize);

		return new RestDTO<BoardGame[]>() 
		{
			Data = await query.ToArrayAsync(),
			PageIndex = pageIndex,
			PageSize = pageSize,
			RecordCount = await _context.BoardGames.CountAsync(),
			Links = new List<LinkDTO> 
			{
				new LinkDTO(
					Url.Action(null, "BoardGames", new { pageIndex, pageSize }, Request.Scheme)!,
					"self",
					"GET"), 
			}
		};
	}
}
