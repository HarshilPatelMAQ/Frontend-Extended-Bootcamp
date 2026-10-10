using ServicePortal.Domain.Interfaces;

namespace ServicePortal.Application.Services;

/// <summary>
/// SLA calculation service implementing business-hour calculations.
/// Business hours: Monday-Friday, 09:00-18:00 (9 hours per day).
/// Demonstrates Inheritance potential and Encapsulation of complex date logic.
/// </summary>
public class SlaCalculationService : ISlaCalculationService
{
    private static readonly TimeSpan BusinessStart = new(9, 0, 0);  // 09:00
    private static readonly TimeSpan BusinessEnd = new(18, 0, 0);   // 18:00
    private const int BusinessHoursPerDay = 9;

    /// <inheritdoc />
    public DateTime CalculateDueDate(DateTime startDate, int businessHours)
    {
        if (businessHours <= 0)
            return startDate;

        var current = startDate;

        // If starting outside business hours, move to next business hour start
        current = MoveToBusinessHours(current);

        int remainingHours = businessHours;

        while (remainingHours > 0)
        {
            // Calculate remaining business hours today
            var endOfBusinessToday = current.Date.Add(BusinessEnd);
            var hoursLeftToday = (endOfBusinessToday - current).TotalHours;

            if (hoursLeftToday <= 0)
            {
                // Move to next business day
                current = GetNextBusinessDay(current.Date.AddDays(1)).Add(BusinessStart);
                continue;
            }

            if (remainingHours <= hoursLeftToday)
            {
                current = current.AddHours(remainingHours);
                remainingHours = 0;
            }
            else
            {
                remainingHours -= (int)Math.Floor(hoursLeftToday);
                current = GetNextBusinessDay(current.Date.AddDays(1)).Add(BusinessStart);
            }
        }

        return current;
    }

    /// <inheritdoc />
    public bool IsBreached(DateTime? dueDate)
    {
        if (!dueDate.HasValue) return false;
        return DateTime.UtcNow > dueDate.Value;
    }

    /// <summary>
    /// Moves a DateTime to the start of the next business period if it's outside business hours.
    /// </summary>
    private static DateTime MoveToBusinessHours(DateTime dateTime)
    {
        var date = GetNextBusinessDay(dateTime.Date);

        if (dateTime.TimeOfDay < BusinessStart)
            return date.Add(BusinessStart);

        if (dateTime.TimeOfDay >= BusinessEnd)
            return GetNextBusinessDay(date.AddDays(1)).Add(BusinessStart);

        // Already in business hours on a business day
        if (IsBusinessDay(dateTime))
            return dateTime;

        return GetNextBusinessDay(dateTime.Date).Add(BusinessStart);
    }

    private static bool IsBusinessDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    private static DateTime GetNextBusinessDay(DateTime date)
    {
        while (!IsBusinessDay(date))
            date = date.AddDays(1);
        return date;
    }
}
