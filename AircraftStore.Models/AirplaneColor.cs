using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aircraft.Models;

public class AirplaneColor
{
    public int Id { get; set; }
    public string? ProductCode { get; set; }

    [Column(TypeName = "decimal(10, 2)")] public decimal FactoryPrice { get; set; }

    [Column(TypeName = "decimal(10, 2)")] public decimal SalePrice { get; set; }

    public int SortOrder { get; set; }
    public int Priority { get; set; } = 1;
    public bool Active { get; set; }
    public string? Url { get; set; }
    public DateTime Created { get; set; }
    public DateTime Edited { get; set; }

    public int AirplaneId { get; set; }
    [ValidateNever] public Airplane? Airplane { get; set; }
    public int ColorId { get; set; }
    [ValidateNever] public Color? Color { get; set; }

    [ValidateNever] public IEnumerable<AirplaneSize>? AirplaneSizes { get; set; }
    [ValidateNever] public IEnumerable<Image>? Images { get; set; }
}