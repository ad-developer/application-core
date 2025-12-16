namespace ApplicationCore.EmailQueue;

public class EmailQueueOptions
    {
        public int PollIntervalSeconds { get; set; } = 5;
        public int BatchSize { get; set; } = 50;
        public int MaxDegreeOfParallelism { get; set; } = 8; // concurrent sends per batch
        public int MaxAttempts { get; set; } = 5;
        public int LockSeconds { get; set; } = 60; // claim duration
        // SMTP config
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpUseSsl { get; set; } = true;
        public string FromAddress { get; set; }
        public string FromName { get; set; }
    }
