using System.ComponentModel.DataAnnotations;

namespace TestWebApplication.Customers.Models;

public class Customer
{
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
}
