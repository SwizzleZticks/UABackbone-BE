namespace UABackbone_Backend.DTOs;

public class LocalUnionDto
{
    public string?  Location        { get; set; }

    public decimal? Wage            { get; set; }

    public decimal? Vacation        { get; set; }

    public decimal? HealthWelfare   { get; set; }

    public decimal? NationalPension { get; set; }

    public decimal? LocalPension    { get; set; }

    public decimal? Annuity         { get; set; }
}