﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aircraft.Models;

public class Color
{
    public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] [Range(0, Int32.MaxValue)] public int Priority { get; set; }

    [ValidateNever] public IEnumerable<AirplaneColor> AirplaneColors { get; set; }
}