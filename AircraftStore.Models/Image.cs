using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aircraft.Models;

public class Image
{
    public int Id { get; set; }
    [Required] public string? Path { get; set; }
    [Required]
    public int SortOrder { get; set; }

    public int AirplaneColorId { get; set; }
    [ValidateNever] public AirplaneColor? AirplaneColor { get; set; }
}