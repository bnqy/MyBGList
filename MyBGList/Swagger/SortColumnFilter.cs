using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using MyBGList.Attributes;


namespace MyBGList.Swagger;

public class SortColumnFilter : IParameterFilter
{
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{

		throw new NotImplementedException();
	}
}
