using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models;

public class ImportViewModel
{
    [Required]
    [Display(Name = "Product list path")]
    public string Path { get; set; } = "";
}
