using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aircraft.Models;

public class OrderDetail
{
    public int Id { get; set; }

    [Required] public int OrderId { get; set; }

    [ValidateNever]
    [ForeignKey("OrderId")]
    public ShopOrder ShopOrder { get; set; }

    [Required] public int AirplaneSizeId { get; set; }

    [ForeignKey("AirplaneSizeId")]
    [ValidateNever]
    public AirplaneSize AirplaneSize { get; set; }

    public int Count { get; set; }
    public decimal PriceEach { get; set; }
}