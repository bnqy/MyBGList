using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBGList.DTO;
using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using MyBGList.Models;

namespace MyBGList.Controllers
{
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
		public async Task<RestDTO<BoardGame[]>> Get()
		{
			var query = _context.BoardGames;
			return new RestDTO<BoardGame[]>() 
			{
				Data = await query.ToArrayAsync(),
				Links = new List<LinkDTO> 
				{
					new LinkDTO(
						Url.Action(null, "BoardGames", null, Request.Scheme)!,
						"self",
						"GET"), 
				}
			};
		}
	}
}
