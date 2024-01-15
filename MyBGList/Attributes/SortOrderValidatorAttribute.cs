using System.ComponentModel.DataAnnotations;

namespace MyBGList.Attributes;

public class SortOrderValidatorAttribute : ValidationAttribute
{
	public string[] AllowedValues { get; set; } = new[] { "ASC", "DESC" };

	public SortOrderValidatorAttribute()
 : base("Value must be one of the following: {0}.") { }
}
