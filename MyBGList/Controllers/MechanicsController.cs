﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.DTO;
using MyBGList.Models;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.ComponentModel.DataAnnotations;
using MyBGList.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using MyBGList.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using MyBGList.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace MyBGList.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class MechanicsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		private readonly ILogger<MechanicsController> _logger;

		private readonly IDistributedCache _distributedCache;


		public MechanicsController(
			ApplicationDbContext context,
			ILogger<MechanicsController> logger,
			IDistributedCache distributedCache)
		{
			_context = context;
			_logger = logger;
			_distributedCache = distributedCache;
		}

		[HttpGet(Name = "GetMechanics")]
		//[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
		[ResponseCache(CacheProfileName = "Any-60")]
		[SwaggerOperation(Summary = "Gets a list of mechanics",
			Description = "Retrieves a list of mechanics with custom\r\npaging, sorting, and filtering rules")]
		public async Task<RestDTO<Mechanic[]>> Get(
			[FromQuery]
		[SwaggerParameter("A DTO object that can be used to customize\r\nsome retrieval parameters")]
		RequestDTO<MechanicDTO> input)
		{
			var query = _context.Mechanics.AsQueryable();
			if (!string.IsNullOrEmpty(input.FilterQuery))
				query = query.Where(b => b.Name.Contains(input.FilterQuery));

			var recordCount = await query.CountAsync();

			Mechanic[]? result = null;

			var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";

			if (!_distributedCache.TryGetValue<Mechanic[]>(cacheKey, out result))
			{
				query = query
					.OrderBy($"{input.SortColumn} {input.SortOrder}")
					.Skip(input.PageIndex * input.PageSize)
					.Take(input.PageSize);

				result = await query.ToArrayAsync();
				_distributedCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
			}

			return new RestDTO<Mechanic[]>()
			{
				Data = result,
				PageIndex = input.PageIndex,
				PageSize = input.PageSize,
				RecordCount = recordCount,
				Links = new List<LinkDTO> {
					new LinkDTO(
						Url.Action(
							null,
							"Mechanics",
							new { input.PageIndex, input.PageSize },
							Request.Scheme)!,
						"self",
						"GET"),
				}
			};
		}

		[Authorize(Roles = RoleNames.Moderator)]
		[HttpPost(Name = "UpdateMechanic")]
		//[ResponseCache(NoStore = true)]
		[ResponseCache(CacheProfileName = "NoCache")]
		[SwaggerOperation(Summary = "Updates the mechanics data", 
			Description = "Updates mechanics data in DB")]
		public async Task<RestDTO<Mechanic?>> Post([SwaggerParameter("Mechanic DTO object contains mechanic data")] MechanicDTO model)
		{
			var mechanic = await _context.Mechanics
				.Where(b => b.Id == model.Id)
				.FirstOrDefaultAsync();
			if (mechanic != null)
			{
				if (!string.IsNullOrEmpty(model.Name))
					mechanic.Name = model.Name;
				mechanic.LastModifiedDate = DateTime.Now;
				_context.Mechanics.Update(mechanic);
				await _context.SaveChangesAsync();
			};

			return new RestDTO<Mechanic?>()
			{
				Data = mechanic,
				Links = new List<LinkDTO>
				{
					new LinkDTO(
							Url.Action(
								null,
								"Mechanics",
								model,
								Request.Scheme)!,
							"self",
							"POST"),
				}
			};
		}

		[Authorize(Roles = RoleNames.Administrator)]
		[HttpDelete(Name = "DeleteMechanic")]
		//[ResponseCache(NoStore = true)]
		[ResponseCache(CacheProfileName = "NoCache")]
		[SwaggerOperation(Summary = "Deletes mechanic data", 
			Description = "Deletes mechanic data in DB by id")]
		public async Task<RestDTO<Mechanic?>> Delete([SwaggerParameter("id of mechanic data in DB")] int id)
		{
			var mechanic = await _context.Mechanics
				.Where(b => b.Id == id)
				.FirstOrDefaultAsync();
			if (mechanic != null)
			{
				_context.Mechanics.Remove(mechanic);
				await _context.SaveChangesAsync();
			};

			return new RestDTO<Mechanic?>()
			{
				Data = mechanic,
				Links = new List<LinkDTO>
				{
					new LinkDTO(
							Url.Action(
								null,
								"Mechanics",
								id,
								Request.Scheme)!,
							"self",
							"DELETE"),
				}
			};
		}
	}
}