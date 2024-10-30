using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class District
{
    public int DistrictId {get;set;}
    public string? DistrictName {get; set;}
}