using Microsoft.EntityFrameworkCore;

namespace ApplicationCore.EmailQueue;

public class EmailQueueDbContext : DbContext
    {
        public DbSet<EmailMessage> EmailMessages { get; set; }

        public EmailQueueDbContext(DbContextOptions<EmailQueueDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailMessage>()
                .HasIndex(e => new { e.Status, e.NextAttemptAt });
            modelBuilder.Entity<EmailMessage>()
                .HasIndex(e => e.ExternalId)
                .IsUnique(false);
        }
    }
