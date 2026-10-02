using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CatalogManager.Domain;

namespace CatalogManager.Models;

public class ProductEditViewModel
{
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = "";

    // Blank means unpriced, which puts the product back into "Needs pricing".
    [Range(0.01, 99_999_999.99, ErrorMessage = "Price must be greater than zero.")]
    [Display(Name = "Price (AUD)")]
    public decimal? Price { get; set; }

    // Read-only details shown alongside the form; not posted back.
    [BindNever]
    public Product? Product { get; set; }
}
