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

	//domain

	[Serial]
	[Authorize(Roles = new[] { RoleNames.Moderator })]
	public async Task<Domain?> UpdateDomain([Service] ApplicationDbContext context,
		DomainDTO model)
	{
		var domain = await context.Domains
			.Where(b => b.Id == model.Id)
			.FirstOrDefaultAsync();

		if (domain != null)
		{
			if (!string.IsNullOrEmpty(model.Name))
				domain.Name = model.Name;

			domain.LastModifiedDate = DateTime.Now;
			context.Domains.Update(domain);
			await context.SaveChangesAsync();
		}
		return domain;
	}

	[Serial]
	[Authorize(Roles = new[] { RoleNames.Administrator })]
	public async Task DeleteDomain([Service] ApplicationDbContext context,
		int id)
	{
		var domain = await context.Domains
			.Where(b => b.Id == id)
			.FirstOrDefaultAsync();

		if (domain != null)
		{
			context.Domains.Remove(domain);
			await context.SaveChangesAsync();
		}
	}
}
