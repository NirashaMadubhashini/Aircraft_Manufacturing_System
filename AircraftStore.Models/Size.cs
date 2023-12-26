using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;

namespace Aircraft.Models;

public class Size
{
    public int Id { get; set; }
    [Required] public string Unit { get; set; }

    public double Value { get; set; }

    [JsonIgnore] [ValidateNever] public IEnumerable<AirplaneSize>? AirplaneSizes { get; set; }
}