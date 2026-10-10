using System.ComponentModel.DataAnnotations;
using ServicePortal.Domain.Enums;

namespace ServicePortal.Web.ViewModels;

/// <summary>
/// ViewModel for creating a new service request with validation.
/// </summary>
public class CreateServiceRequestViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    [Display(Name = "Priority")]
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;

    [Display(Name = "Preferred Contact Method")]
    public string PreferredContactMethod { get; set; } = "Email";

    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; }

    /// <summary>
    /// Duplicate warning acknowledgement flag.
    /// </summary>
    public bool AcknowledgeDuplicate { get; set; }
}

/// <summary>
/// ViewModel for service request list with filtering, sorting, and pagination.
/// </summary>
public class ServiceRequestListViewModel
{
    public List<ServiceRequestSummaryViewModel> Requests { get; set; } = new();

    // Filters
    public string? SearchTerm { get; set; }
    public RequestStatus? StatusFilter { get; set; }
    public int? CategoryFilter { get; set; }
    public RequestPriority? PriorityFilter { get; set; }
    public int? TeamFilter { get; set; }
    public string? AssigneeFilter { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SlaStateFilter { get; set; } // "breached", "warning", "ok"
    public string? OwnershipFilter { get; set; } // "mine", "team", "all"

    // Sorting
    public string SortBy { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "desc";

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public class ServiceRequestSummaryViewModel
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public RequestPriority Priority { get; set; }
    public RequestStatus Status { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public bool IsResponseSlaBreached { get; set; }
    public bool IsResolutionSlaBreached { get; set; }
    public string SlaState { get; set; } = "ok"; // "ok", "warning", "breached"
}

/// <summary>
/// ViewModel for viewing a single service request with full details.
/// </summary>
public class ServiceRequestDetailViewModel
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public RequestPriority Priority { get; set; }
    public RequestStatus Status { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterId { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public string? AssigneeId { get; set; }
    public string PreferredContactMethod { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public bool IsResponseSlaBreached { get; set; }
    public bool IsResolutionSlaBreached { get; set; }
    public string? ResolutionSummary { get; set; }
    public ResolutionCategory? ResolutionCategoryValue { get; set; }
    public byte[]? RowVersion { get; set; }

    public List<CommentViewModel> Comments { get; set; } = new();
    public List<AttachmentViewModel> Attachments { get; set; } = new();
    public List<StatusHistoryViewModel> StatusHistory { get; set; } = new();
    public List<AssignmentHistoryViewModel> AssignmentHistory { get; set; } = new();
    public List<RequestStatus> ValidTransitions { get; set; } = new();

    // For forms
    public string? NewCommentBody { get; set; }
    public bool NewCommentIsInternal { get; set; }
    public List<IFormFile>? NewAttachments { get; set; }

    // For status transition
    public RequestStatus? NewStatus { get; set; }
    public string? TransitionReason { get; set; }
    public string? ResolutionSummaryInput { get; set; }
    public ResolutionCategory? ResolutionCategoryInput { get; set; }

    // For assignment
    public string? NewAssigneeId { get; set; }
    public string? AssignmentReason { get; set; }

    // Role info
    public bool IsRequester { get; set; }
    public bool IsStaff { get; set; }
    public bool IsTeamLead { get; set; }
    public bool IsAdmin { get; set; }
    public bool CanReopen { get; set; }
}

public class CommentViewModel
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<AttachmentViewModel> Attachments { get; set; } = new();
}

public class AttachmentViewModel
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploaderName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class StatusHistoryViewModel
{
    public RequestStatus FromStatus { get; set; }
    public RequestStatus ToStatus { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime ChangedDate { get; set; }
}

public class AssignmentHistoryViewModel
{
    public string? PreviousAssigneeName { get; set; }
    public string? NewAssigneeName { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime AssignedDate { get; set; }
}

/// <summary>
/// Dashboard ViewModels for different roles.
/// </summary>
public class RequesterDashboardViewModel
{
    public int OpenCount { get; set; }
    public int WaitingCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
    public List<ServiceRequestSummaryViewModel> RecentRequests { get; set; } = new();
}

public class AgentDashboardViewModel
{
    public int AssignedCount { get; set; }
    public int UnassignedQueueCount { get; set; }
    public int DueSoonCount { get; set; }
    public int BreachedCount { get; set; }
    public List<ServiceRequestSummaryViewModel> AssignedRequests { get; set; } = new();
    public List<ServiceRequestSummaryViewModel> UnassignedQueue { get; set; } = new();
}

public class TeamLeadDashboardViewModel
{
    public Dictionary<string, int> RequestsByStatus { get; set; } = new();
    public Dictionary<string, int> RequestsByPriority { get; set; } = new();
    public Dictionary<string, int> AgentWorkload { get; set; } = new();
    public double SlaComplianceRate { get; set; }
    public double AverageResolutionHours { get; set; }
    public List<ServiceRequestSummaryViewModel> BreachedRequests { get; set; } = new();
}

public class AdminDashboardViewModel
{
    public int ActiveUsers { get; set; }
    public int TotalRequests { get; set; }
    public Dictionary<string, int> RequestsByCategory { get; set; } = new();
    public Dictionary<string, int> RequestsByStatus { get; set; } = new();
    public int UnreadNotifications { get; set; }
}
