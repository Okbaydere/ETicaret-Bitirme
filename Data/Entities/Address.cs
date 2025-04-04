using Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class Address
{
    public int Id { get; set; }

    [Required] public string Title { get; set; } = string.Empty;

    [Required] public string FullAddress { get; set; } = string.Empty;

    [Required] public string City { get; set; } = string.Empty;

    [Required] public int UserId { get; set; }

    public virtual AppUser User { get; set; } = null!;

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;
}