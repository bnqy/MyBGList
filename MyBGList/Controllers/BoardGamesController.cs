using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;

namespace MyBGList.Controllers
{
	//[Route("api/[controller]")]
	[Route("[controller]")]
	[ApiController]
	public class BoardGamesController : ControllerBase
	{
		private readonly ILogger<BoardGamesController> _logger;

		public BoardGamesController(ILogger<BoardGamesController> logger)
		{
			_logger = logger;
		}

		[HttpGet(Name = "GetBoardGames")]
		public IEnumerable<BoardGame> Get()
		{
			return new[]
			{
				new BoardGame()
				{
					Id = 1,
					Name = "Dua Lipa",
					Year = 1995
				},

				new BoardGame()
				{
					Id = 2,
					Name = "Bene Om",
					Year = 2003
				},

				new BoardGame()
				{
					Id = 3,
					Name = "Lana Del Rey",
					Year = 1985
				}
			};
		}
	}
}
