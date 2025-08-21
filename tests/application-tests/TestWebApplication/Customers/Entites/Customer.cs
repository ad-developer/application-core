using System.ComponentModel.DataAnnotations;
using ApplicationCore.DataPersistence;

namespace TestWebApplication.Customers.Entites;

public class Customer : IEntity<Guid>
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    public required string Address { get; set; }

    [Required]
    public required string City { get; set; }

    [Required]
    public required string State { get; set; }

    [Required]
    public required string ZipCode { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [Phone]
    public required string PhoneNumber { get; set; }

    [Required]
    public required string AddedBy { get; set; }

    [Required]
    public DateTime AddedDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;
}
