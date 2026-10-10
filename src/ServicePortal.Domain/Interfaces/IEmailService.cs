namespace ServicePortal.Domain.Interfaces;

/// <summary>
/// Interface for email/notification sending. Design allows replacement with a real provider.
/// Implements Dependency Inversion Principle - high-level modules depend on abstraction.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email notification. May use mocked/local SMTP in development.
    /// Failures must not roll back the primary business transaction.
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body);
}
