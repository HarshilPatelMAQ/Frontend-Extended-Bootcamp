using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServicePortal.Domain.Entities;
using ServicePortal.Domain.Enums;
using ServicePortal.Domain.Interfaces;
using ServicePortal.Infrastructure.Data;
using ServicePortal.Infrastructure.Identity;
using ServicePortal.Web.ViewModels;

namespace ServicePortal.Web.Controllers;

[Authorize]
public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWorkflowService _workflowService;
    private readonly ISlaCalculationService _slaService;

    public ServiceRequestsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWorkflowService workflowService,
        ISlaCalculationService slaService)
    {
        _context = context;
        _userManager = userManager;
        _workflowService = workflowService;
        _slaService = slaService;
    }

    // GET: ServiceRequests
    public async Task<IActionResult> Index(
        string? searchTerm,
        RequestStatus? statusFilter,
        RequestPriority? priorityFilter,
        int? categoryFilter,
        string? ownershipFilter,
        int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var isStaff = await _userManager.IsInRoleAsync(user, "Administrator") ||
                      await _userManager.IsInRoleAsync(user, "TeamLead") ||
                      await _userManager.IsInRoleAsync(user, "SupportAgent");

        IQueryable<ServiceRequest> query = _context.ServiceRequests
            .Include(r => r.Category);

        // Filter by ownership if requester or selected filter
        if (!isStaff)
        {
            query = query.Where(r => r.RequesterId == user.Id);
        }
        else if (ownershipFilter == "mine")
        {
            query = query.Where(r => r.AssigneeId == user.Id);
        }

        // Apply Search Term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(term) ||
                                     r.ReferenceNumber.ToLower().Contains(term) ||
                                     r.Description.ToLower().Contains(term));
        }

        // Apply Filters
        if (statusFilter.HasValue)
            query = query.Where(r => r.Status == statusFilter.Value);

        if (priorityFilter.HasValue)
            query = query.Where(r => r.Priority == priorityFilter.Value);

        if (categoryFilter.HasValue)
            query = query.Where(r => r.CategoryId == categoryFilter.Value);

        var totalItems = await query.CountAsync();
        var pageSize = 10;

        var rawItems = await query
            .OrderByDescending(r => r.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userIds = rawItems.Select(r => r.RequesterId)
            .Concat(rawItems.Where(r => r.AssigneeId != null).Select(r => r.AssigneeId!))
            .Distinct();

        var userMap = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.Email ?? "Unknown");

        var items = rawItems.Select(r => new ServiceRequestSummaryViewModel
        {
            Id = r.Id,
            ReferenceNumber = r.ReferenceNumber,
            Title = r.Title,
            CategoryName = r.Category?.Name ?? "General",
            Priority = r.Priority,
            Status = r.Status,
            RequesterName = userMap.TryGetValue(r.RequesterId, out var rName) ? rName : "Unknown",
            AssigneeName = r.AssigneeId != null && userMap.TryGetValue(r.AssigneeId, out var aName) ? aName : "Unassigned",
            CreatedDate = r.CreatedDate,
            ResponseDueDate = r.ResponseDueDate,
            ResolutionDueDate = r.ResolutionDueDate,
            IsResponseSlaBreached = r.IsResponseSlaBreached,
            IsResolutionSlaBreached = r.IsResolutionSlaBreached,
            SlaState = (r.IsResponseSlaBreached || r.IsResolutionSlaBreached) ? "breached" :
                       (r.ResolutionDueDate.HasValue && r.ResolutionDueDate.Value < DateTime.UtcNow.AddHours(2)) ? "warning" : "ok"
        }).ToList();

        ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
        ViewBag.IsStaff = isStaff;

        // KPI Counts for Dashboard
        ViewBag.TotalCount = await _context.ServiceRequests.CountAsync();
        ViewBag.OpenCount = await _context.ServiceRequests.CountAsync(r => r.Status != RequestStatus.Closed);
        ViewBag.BreachedCount = await _context.ServiceRequests.CountAsync(r => r.IsResponseSlaBreached || r.IsResolutionSlaBreached);
        ViewBag.ResolvedCount = await _context.ServiceRequests.CountAsync(r => r.Status == RequestStatus.Resolved);

        var model = new ServiceRequestListViewModel
        {
            Requests = items,
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            PriorityFilter = priorityFilter,
            CategoryFilter = categoryFilter,
            OwnershipFilter = ownershipFilter,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View(model);
    }

    // GET: ServiceRequests/ExportCsv
    public async Task<IActionResult> ExportCsv(
        string? searchTerm,
        RequestStatus? statusFilter,
        RequestPriority? priorityFilter,
        int? categoryFilter,
        string? ownershipFilter)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var isStaff = await _userManager.IsInRoleAsync(user, "Administrator") ||
                      await _userManager.IsInRoleAsync(user, "TeamLead") ||
                      await _userManager.IsInRoleAsync(user, "SupportAgent");

        IQueryable<ServiceRequest> query = _context.ServiceRequests
            .Include(r => r.Category);

        if (!isStaff)
        {
            query = query.Where(r => r.RequesterId == user.Id);
        }
        else if (ownershipFilter == "mine")
        {
            query = query.Where(r => r.AssigneeId == user.Id);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(term) ||
                                     r.ReferenceNumber.ToLower().Contains(term) ||
                                     r.Description.ToLower().Contains(term));
        }

        if (statusFilter.HasValue) query = query.Where(r => r.Status == statusFilter.Value);
        if (priorityFilter.HasValue) query = query.Where(r => r.Priority == priorityFilter.Value);
        if (categoryFilter.HasValue) query = query.Where(r => r.CategoryId == categoryFilter.Value);

        var requests = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("ReferenceNumber,Title,Category,Priority,Status,CreatedDate,ResponseDueDate,ResolutionDueDate,ResponseSlaBreached,ResolutionSlaBreached");

        foreach (var r in requests)
        {
            var titleEscaped = $"\"{r.Title.Replace("\"", "\"\"")}\"";
            var catEscaped = $"\"{r.Category?.Name.Replace("\"", "\"\"")}\"";
            builder.AppendLine($"{r.ReferenceNumber},{titleEscaped},{catEscaped},{r.Priority},{r.Status},{r.CreatedDate:yyyy-MM-dd HH:mm},{r.ResponseDueDate:yyyy-MM-dd HH:mm},{r.ResolutionDueDate:yyyy-MM-dd HH:mm},{r.IsResponseSlaBreached},{r.IsResolutionSlaBreached}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
        return File(bytes, "text/csv", $"ServiceRequests_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    // GET: ServiceRequests/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var request = await _context.ServiceRequests
            .Include(r => r.Category)
            .Include(r => r.Comments)
            .Include(r => r.StatusHistory)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (request == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var isStaff = await _userManager.IsInRoleAsync(user, "Administrator") ||
                      await _userManager.IsInRoleAsync(user, "TeamLead") ||
                      await _userManager.IsInRoleAsync(user, "SupportAgent");

        if (!isStaff && request.RequesterId != user.Id)
        {
            return Forbid();
        }

        var allowedNextStatuses = _workflowService.GetValidTransitions(request.Status).ToList();

        var requesterUser = await _userManager.FindByIdAsync(request.RequesterId);
        var assigneeUser = request.AssigneeId != null ? await _userManager.FindByIdAsync(request.AssigneeId) : null;

        // Fetch support staff for assignee dropdown
        var staffUsers = await _userManager.GetUsersInRoleAsync("SupportAgent");
        var leadUsers = await _userManager.GetUsersInRoleAsync("TeamLead");
        var adminUsers = await _userManager.GetUsersInRoleAsync("Administrator");

        var allStaff = staffUsers.Concat(leadUsers).Concat(adminUsers)
            .DistinctBy(u => u.Id)
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.DisplayName ?? u.Email} ({u.Email})"
            })
            .ToList();

        var model = new ServiceRequestDetailViewModel
        {
            Id = request.Id,
            ReferenceNumber = request.ReferenceNumber,
            Title = request.Title,
            Description = request.Description,
            CategoryName = request.Category?.Name ?? "General",
            CategoryId = request.CategoryId,
            Priority = request.Priority,
            Status = request.Status,
            RequesterId = request.RequesterId,
            RequesterName = requesterUser?.DisplayName ?? requesterUser?.Email ?? "Unknown",
            RequesterEmail = requesterUser?.Email ?? "",
            AssigneeId = request.AssigneeId,
            AssigneeName = assigneeUser?.DisplayName ?? assigneeUser?.Email ?? "Unassigned",
            PreferredContactMethod = request.PreferredContactMethod,
            CreatedDate = request.CreatedDate,
            AssignedDate = request.AssignedDate,
            FirstResponseDate = request.FirstResponseDate,
            ResolvedDate = request.ResolvedDate,
            ClosedDate = request.ClosedDate,
            ResponseDueDate = request.ResponseDueDate,
            ResolutionDueDate = request.ResolutionDueDate,
            IsResponseSlaBreached = request.IsResponseSlaBreached,
            IsResolutionSlaBreached = request.IsResolutionSlaBreached,
            ResolutionSummary = request.ResolutionSummary,
            Comments = request.Comments
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    AuthorName = c.AuthorId,
                    Body = c.Body,
                    IsInternal = c.IsInternal,
                    CreatedDate = c.CreatedDate
                }).ToList(),
            AllowedNextStatuses = allowedNextStatuses,
            StaffList = allStaff,
            IsStaff = isStaff
        };

        return View(model);
    }

    // GET: ServiceRequests/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.CategoryId = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
        return View(new CreateServiceRequestViewModel());
    }

    // POST: ServiceRequests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceRequestViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        // Calculate SLA due dates based on priority
        var slaPolicy = await _context.SlaPolicies.FirstOrDefaultAsync(p => p.Priority == model.Priority && p.IsActive);
        var responseHours = slaPolicy?.ResponseTargetHours ?? 8;
        var resolutionHours = slaPolicy?.ResolutionTargetHours ?? 24;

        var responseDue = _slaService.CalculateDueDate(DateTime.UtcNow, responseHours);
        var resolutionDue = _slaService.CalculateDueDate(DateTime.UtcNow, resolutionHours);

        // Generate Reference Number
        var requestCount = await _context.ServiceRequests.CountAsync() + 1;
        var referenceNumber = $"SR-{DateTime.UtcNow.Year}-{requestCount:D6}";

        var request = new ServiceRequest
        {
            ReferenceNumber = referenceNumber,
            Title = model.Title,
            Description = model.Description,
            CategoryId = model.CategoryId,
            Priority = model.Priority,
            PreferredContactMethod = model.PreferredContactMethod,
            RequesterId = user.Id,
            Status = RequestStatus.New,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            ResponseDueDate = responseDue,
            ResolutionDueDate = resolutionDue
        };

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        // Add initial status history record
        _context.RequestStatusHistory.Add(new RequestStatusHistory
        {
            ServiceRequestId = request.Id,
            FromStatus = RequestStatus.New,
            ToStatus = RequestStatus.New,
            ChangedById = user.Id,
            Reason = "Service request created.",
            ChangedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Service Request {request.ReferenceNumber} has been created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // POST: ServiceRequests/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, RequestStatus newStatus, string? reason)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var request = await _context.ServiceRequests.FindAsync(id);
        if (request == null) return NotFound();

        if (!_workflowService.IsValidTransition(request.Status, newStatus))
        {
            TempData["ErrorMessage"] = $"Invalid status transition from {request.Status} to {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var oldStatus = request.Status;
        request.Status = newStatus;
        request.LastUpdatedDate = DateTime.UtcNow;

        if (newStatus == RequestStatus.Resolved)
        {
            request.ResolvedDate = DateTime.UtcNow;
        }
        else if (newStatus == RequestStatus.Closed)
        {
            request.ClosedDate = DateTime.UtcNow;
        }

        _context.RequestStatusHistory.Add(new RequestStatusHistory
        {
            ServiceRequestId = request.Id,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangedById = user.Id,
            Reason = reason ?? $"Status changed to {newStatus}",
            ChangedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Status updated to {newStatus} successfully!";

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: ServiceRequests/Assign
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, string assigneeId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var request = await _context.ServiceRequests.FindAsync(id);
        if (request == null) return NotFound();

        var oldAssigneeId = request.AssigneeId;
        request.AssigneeId = assigneeId;
        request.AssignedDate = DateTime.UtcNow;
        if (request.Status == RequestStatus.New)
        {
            request.Status = RequestStatus.Assigned;
        }
        request.LastUpdatedDate = DateTime.UtcNow;

        _context.AssignmentHistory.Add(new AssignmentHistory
        {
            ServiceRequestId = request.Id,
            PreviousAssigneeId = oldAssigneeId,
            NewAssigneeId = assigneeId,
            AssignedById = user.Id,
            AssignedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Request assigned successfully!";

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: ServiceRequests/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string commentBody, bool isInternal)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrWhiteSpace(commentBody))
        {
            TempData["ErrorMessage"] = "Comment body cannot be empty.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var comment = new RequestComment
        {
            ServiceRequestId = id,
            AuthorId = user.DisplayName ?? user.Email ?? user.Id,
            Body = commentBody,
            IsInternal = isInternal,
            CreatedDate = DateTime.UtcNow
        };

        _context.RequestComments.Add(comment);

        // Update first response date if staff member responds for the first time
        var request = await _context.ServiceRequests.FindAsync(id);
        if (request != null && !request.FirstResponseDate.HasValue)
        {
            var isStaff = await _userManager.IsInRoleAsync(user, "Administrator") ||
                          await _userManager.IsInRoleAsync(user, "TeamLead") ||
                          await _userManager.IsInRoleAsync(user, "SupportAgent");
            if (isStaff)
            {
                request.FirstResponseDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Comment posted successfully!";

        return RedirectToAction(nameof(Details), new { id });
    }
}
