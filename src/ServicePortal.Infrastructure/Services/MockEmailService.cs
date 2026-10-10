using ServicePortal.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ServicePortal.Infrastructure.Services;

/// <summary>
/// Mocked email service for development. Logs email details instead of sending.
/// Implements IEmailService interface - can be swapped with a real provider via DI.
/// Demonstrates Dependency Inversion Principle (SOLID).
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Log the email instead of sending - notification failures must not affect business transactions
        _logger.LogInformation(
            "📧 [MOCK EMAIL] To: {To} | Subject: {Subject} | Body: {Body}",
            to, subject, body);
        return Task.CompletedTask;
    }
}
