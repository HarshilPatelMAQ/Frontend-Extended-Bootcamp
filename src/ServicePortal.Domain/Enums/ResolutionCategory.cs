namespace ServicePortal.Domain.Enums;

/// <summary>
/// Resolution categories for resolved service requests.
/// </summary>
public enum ResolutionCategory
{
    Fixed = 0,
    WorkingAsDesigned = 1,
    Duplicate = 2,
    CannotReproduce = 3,
    ThirdPartyIssue = 4,
    UserEducation = 5,
    Other = 6
}
