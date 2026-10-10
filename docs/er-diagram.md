# Entity Relationship Diagram (ERD)

The following diagram illustrates the relational data model for the **Smart Service Request Management Portal (SSRMP)** using Entity Framework Core & SQL Server.

```mermaid
erDiagram
    ApplicationUser ||--o{ ServiceRequest : "submits as Requester"
    ApplicationUser ||--o{ ServiceRequest : "assigned as Assignee"
    ApplicationUser }|--|| SupportTeam : "belongs to"
    
    SupportTeam ||--o{ Category : "owns"
    
    Category ||--o{ ServiceRequest : "categorizes"
    
    ServiceRequest ||--o{ RequestComment : "has comments"
    ServiceRequest ||--o{ RequestAttachment : "has attachments"
    ServiceRequest ||--o{ RequestStatusHistory : "tracks status changes"
    ServiceRequest ||--o{ AssignmentHistory : "tracks assignment history"
    ServiceRequest ||--o{ Notification : "triggers notifications"
    
    RequestComment ||--o{ RequestAttachment : "contains optional attachments"
    
    SlaPolicy ||--o{ ServiceRequest : "defines target SLAs by Priority"
    
    ApplicationUser ||--o{ AuditLog : "acts in audit events"

    ApplicationUser {
        string Id PK
        string DisplayName
        bool IsActive
        int SupportTeamId FK
        string Email
        string UserName
    }

    SupportTeam {
        int Id PK
        string Name
        string Description
        bool IsActive
    }

    Category {
        int Id PK
        string Name
        int SupportTeamId FK
        bool IsActive
    }

    ServiceRequest {
        int Id PK
        string ReferenceNumber UK
        string Title
        string Description
        int CategoryId FK
        int Priority
        int Status
        string RequesterId FK
        string AssigneeId FK
        string PreferredContactMethod
        DateTime CreatedDate
        DateTime ResponseDueDate
        DateTime ResolutionDueDate
        bool IsResponseSlaBreached
        bool IsResolutionSlaBreached
        string ResolutionSummary
        byte[] RowVersion
    }

    RequestComment {
        int Id PK
        int ServiceRequestId FK
        string AuthorId
        bool IsInternal
        string Body
        DateTime CreatedDate
    }

    RequestAttachment {
        int Id PK
        int ServiceRequestId FK
        int CommentId FK
        string OriginalFileName
        string StoredFileName
        string ContentType
        long FileSize
        string UploaderId
        DateTime CreatedDate
    }

    RequestStatusHistory {
        int Id PK
        int ServiceRequestId FK
        int FromStatus
        int ToStatus
        string ChangedById
        string Reason
        DateTime ChangedDate
    }

    AssignmentHistory {
        int Id PK
        int ServiceRequestId FK
        string PreviousAssigneeId
        string NewAssigneeId
        string AssignedById
        string Reason
        DateTime AssignedDate
    }

    SlaPolicy {
        int Id PK
        int Priority UK
        int ResponseTargetHours
        int ResolutionTargetHours
        bool IsActive
    }

    Notification {
        int Id PK
        string RecipientId FK
        int Type
        string Message
        int ServiceRequestId FK
        bool IsRead
        DateTime CreatedDate
    }

    AuditLog {
        int Id PK
        string Actor
        string Action
        string EntityType
        string EntityId
        string SafeChangeSummary
        string CorrelationId
        DateTime Timestamp
    }
```

---

## Data Model Relationship Details

| Relationship | Parent Entity | Child Entity | Foreign Key | Delete Behavior |
| :--- | :--- | :--- | :--- | :--- |
| **Request Category** | `Category` | `ServiceRequest` | `CategoryId` | `Restrict` |
| **Team Categories** | `SupportTeam` | `Category` | `SupportTeamId` | `Restrict` |
| **Request Comments** | `ServiceRequest` | `RequestComment` | `ServiceRequestId` | `Cascade` |
| **Request Attachments** | `ServiceRequest` | `RequestAttachment` | `ServiceRequestId` | `Restrict` (Prevents SQL Server multiple cascade paths) |
| **Comment Attachments**| `RequestComment` | `RequestAttachment` | `CommentId` | `SetNull` |
| **Status Audit History**| `ServiceRequest` | `RequestStatusHistory` | `ServiceRequestId` | `Cascade` |
| **Assignment Audit** | `ServiceRequest` | `AssignmentHistory` | `ServiceRequestId` | `Cascade` |
| **Notifications** | `ServiceRequest` | `Notification` | `ServiceRequestId` | `SetNull` |
