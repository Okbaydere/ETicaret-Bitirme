using System.ComponentModel.DataAnnotations;
using Data.Identity;

namespace Data.Entities;

public class Address
{
    public int Id { get; set; }

    [Required] public string Title { get; set; }

    [Required] public string FullAddress { get; set; }

    [Required] public string City { get; set; }

    [Required] public int UserId { get; set; }

    public virtual AppUser User { get; set; }

    public bool IsDefault { get; set; }
}