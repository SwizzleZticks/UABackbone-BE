using System.ComponentModel.DataAnnotations;
using UABackbone_Backend.DTOs;

public class LocalUnionDto
{
    [Required]
    public required short Local { get; set; }
    [Required]
    public required string Location { get; set; }
    [Required]
    public required decimal Wage { get; set; }
    [Required]
    public required decimal? Vacation { get; set; }
    [Required]
    public required decimal? HealthWelfare { get; set; }
    [Required]
    public required decimal? NationalPension { get; set; }
    [Required]
    public required decimal? LocalPension { get; set; }
    [Required]
    public required decimal? Annuity { get; set; }
    
}