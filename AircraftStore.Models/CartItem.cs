﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aircraft.Models;

public class CartItem
{
    public int Id { get; set; }
    [Required, Range(1, int.MaxValue)] public int AirplaneSizeId { get; set; }

    [ValidateNever]
    [ForeignKey("AirplaneSizeId")]
    public AirplaneSize AirplaneSize { get; set; }

    [Range(1, 999)] public int Count { get; set; }
    public decimal PriceEach { get; set; }

    public string? ApplicationUserId { get; set; }

    [ForeignKey("ApplicationUserId")]
    [ValidateNever]
    public ApplicationUser? ApplicationUser { get; set; }
}