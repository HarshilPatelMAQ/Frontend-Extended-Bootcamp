namespace ServicePortal.Domain.Interfaces;

/// <summary>
/// Interface for SLA calculation service.
/// Abstracts business-hour calculations for testability.
/// </summary>
public interface ISlaCalculationService
{
    /// <summary>
    /// Calculates the due date by adding business hours (Mon-Fri 09:00-18:00).
    /// </summary>
    DateTime CalculateDueDate(DateTime startDate, int businessHours);

    /// <summary>
    /// Checks if a due date has been breached.
    /// </summary>
    bool IsBreached(DateTime? dueDate);
}
