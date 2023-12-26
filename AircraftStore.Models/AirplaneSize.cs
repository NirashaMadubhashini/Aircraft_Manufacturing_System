using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aircraft.Models;

public class AirplaneSize
{
    public int Id { get; set; }
    [Required] public int Quantity { get; set; }
    [Required] public int AirplaneColorId { get; set; }

    [ForeignKey("AirplaneColorId")]
    [ValidateNever]
    public AirplaneColor AirplaneColor { get; set; }

    [Required] public int SizeId { get; set; }
    [ForeignKey("SizeId")] [ValidateNever] public Size Size { get; set; }

    [ValidateNever] public IEnumerable<OrderDetail>? OrderDetails { get; set; }
}