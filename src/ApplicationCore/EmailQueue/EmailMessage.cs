using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.EmailQueue;

public class EmailMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Application-provided id for idempotency (optional)
    [MaxLength(200)]
    public string ExternalId { get; set; }

    [Required, MaxLength(200)]
    public string To { get; set; }

    [MaxLength(200)]
    public string Cc { get; set; }

    [MaxLength(200)]
    public string Bcc { get; set; }

    [Required, MaxLength(500)]
    public string Subject { get; set; }

    [Required]
    public string Body { get; set; } // HTML or plain text; add flags if needed

    public EmailStatus Status { get; set; } = EmailStatus.Pending;

    public int AttemptCount { get; set; } = 0;

    public DateTime? NextAttemptAt { get; set; }

    // Locking fields for distributed workers
    public string LockedBy { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public string LastError { get; set; }

    // concurrency token for optimistic concurrency
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
