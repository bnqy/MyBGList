﻿using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using MyBGList.Attributes;


namespace MyBGList.Swagger;

public class SortColumnFilter : IParameterFilter
{
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{
		var attributes = context.ParameterInfo?
			.GetCustomAttributes(true)
			.OfType<SortColumnValidatorAttribute>();

		if (attributes != null)
		{
			foreach (var attribute in attributes)
			{
				var pattern = attribute.EntityType
					.GetProperties()
					.Select(p => p.Name);

				parameter.Schema.Extensions.Add(
					"pattern",
					new OpenApiString(string.Join("|", pattern.Select(v => $"^{v}$")))
					);
			}
		}
	}
}
