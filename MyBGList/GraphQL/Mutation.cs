using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyBGList.Constants;
using MyBGList.DTO;
using MyBGList.Models;


namespace MyBGList.GraphQL;

public class Mutation
{
	[Serial]
	[Authorize(Roles = new[] { RoleNames.Moderator })]
	public async Task<BoardGame?> UpdateBoardGame([Service] ApplicationDbContext context,
		BoardGameDTO model)
	{
		var boardgame = await context.BoardGames
			.Where(b => b.Id == model.Id)
			.FirstOrDefaultAsync();

		if (boardgame != null)
		{
			if (!string.IsNullOrEmpty(model.Name))
				boardgame.Name = model.Name;

			if (model.Year.HasValue && model.Year.Value > 0)
				boardgame.Year = model.Year.Value;

			boardgame.LastModifiedDate = DateTime.Now;
			context.BoardGames.Update(boardgame);
			await context.SaveChangesAsync();
		}
		return boardgame;
	}

	[Serial]
	[Authorize(Roles = new[] { RoleNames.Administrator })]
	public async Task DeleteBoardGame([Service] ApplicationDbContext context,
		int id)
	{
		var boardgame = await context.BoardGames
			.Where(b => b.Id == id)
			.FirstOrDefaultAsync();

		if (boardgame != null)
		{
			context.BoardGames.Remove(boardgame);
			await context.SaveChangesAsync();
		}
	}
}
