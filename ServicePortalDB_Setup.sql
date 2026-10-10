IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    [SupportTeamId] int NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [Actor] nvarchar(200) NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(50) NULL,
    [Timestamp] datetime2 NOT NULL,
    [ChangeSummary] nvarchar(2000) NULL,
    [CorrelationId] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [SlaPolicies] (
    [Id] int NOT NULL IDENTITY,
    [Priority] int NOT NULL,
    [ResponseTargetHours] int NOT NULL,
    [ResolutionTargetHours] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SlaPolicies] PRIMARY KEY ([Id])
);

CREATE TABLE [SupportTeams] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SupportTeams] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [SupportTeamId] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_SupportTeams_SupportTeamId] FOREIGN KEY ([SupportTeamId]) REFERENCES [SupportTeams] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ServiceRequests] (
    [Id] int NOT NULL IDENTITY,
    [ReferenceNumber] nvarchar(20) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [CategoryId] int NOT NULL,
    [Priority] int NOT NULL,
    [Status] int NOT NULL,
    [RequesterId] nvarchar(450) NOT NULL,
    [AssigneeId] nvarchar(450) NULL,
    [PreferredContactMethod] nvarchar(50) NOT NULL,
    [ResponseDueDate] datetime2 NULL,
    [ResolutionDueDate] datetime2 NULL,
    [IsResponseSlaBreached] bit NOT NULL,
    [IsResolutionSlaBreached] bit NOT NULL,
    [ResolutionSummary] nvarchar(2000) NULL,
    [ResolutionCategoryValue] int NULL,
    [CreatedDate] datetime2 NOT NULL,
    [AssignedDate] datetime2 NULL,
    [FirstResponseDate] datetime2 NULL,
    [ResolvedDate] datetime2 NULL,
    [ClosedDate] datetime2 NULL,
    [LastUpdatedDate] datetime2 NOT NULL,
    [RowVersion] rowversion NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceRequests_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [AssignmentHistory] (
    [Id] int NOT NULL IDENTITY,
    [ServiceRequestId] int NOT NULL,
    [PreviousAssigneeId] nvarchar(max) NULL,
    [NewAssigneeId] nvarchar(max) NULL,
    [AssignedById] nvarchar(max) NOT NULL,
    [Reason] nvarchar(500) NULL,
    [AssignedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_AssignmentHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AssignmentHistory_ServiceRequests_ServiceRequestId] FOREIGN KEY ([ServiceRequestId]) REFERENCES [ServiceRequests] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [RecipientId] nvarchar(450) NOT NULL,
    [Type] int NOT NULL,
    [Message] nvarchar(500) NOT NULL,
    [ServiceRequestId] int NULL,
    [IsRead] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_ServiceRequests_ServiceRequestId] FOREIGN KEY ([ServiceRequestId]) REFERENCES [ServiceRequests] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [RequestComments] (
    [Id] int NOT NULL IDENTITY,
    [ServiceRequestId] int NOT NULL,
    [AuthorId] nvarchar(max) NOT NULL,
    [IsInternal] bit NOT NULL,
    [Body] nvarchar(max) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_RequestComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RequestComments_ServiceRequests_ServiceRequestId] FOREIGN KEY ([ServiceRequestId]) REFERENCES [ServiceRequests] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RequestStatusHistory] (
    [Id] int NOT NULL IDENTITY,
    [ServiceRequestId] int NOT NULL,
    [FromStatus] int NOT NULL,
    [ToStatus] int NOT NULL,
    [ChangedById] nvarchar(max) NOT NULL,
    [Reason] nvarchar(1000) NULL,
    [ChangedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_RequestStatusHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RequestStatusHistory_ServiceRequests_ServiceRequestId] FOREIGN KEY ([ServiceRequestId]) REFERENCES [ServiceRequests] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RequestAttachments] (
    [Id] int NOT NULL IDENTITY,
    [ServiceRequestId] int NOT NULL,
    [CommentId] int NULL,
    [OriginalFileName] nvarchar(255) NOT NULL,
    [StoredFileName] nvarchar(255) NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [FileSize] bigint NOT NULL,
    [UploaderId] nvarchar(max) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_RequestAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RequestAttachments_RequestComments_CommentId] FOREIGN KEY ([CommentId]) REFERENCES [RequestComments] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_RequestAttachments_ServiceRequests_ServiceRequestId] FOREIGN KEY ([ServiceRequestId]) REFERENCES [ServiceRequests] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_AssignmentHistory_ServiceRequestId] ON [AssignmentHistory] ([ServiceRequestId]);

CREATE INDEX [IX_AuditLogs_CorrelationId] ON [AuditLogs] ([CorrelationId]);

CREATE INDEX [IX_AuditLogs_EntityType] ON [AuditLogs] ([EntityType]);

CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);

CREATE INDEX [IX_Categories_SupportTeamId] ON [Categories] ([SupportTeamId]);

CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);

CREATE INDEX [IX_Notifications_RecipientId] ON [Notifications] ([RecipientId]);

CREATE INDEX [IX_Notifications_ServiceRequestId] ON [Notifications] ([ServiceRequestId]);

CREATE INDEX [IX_RequestAttachments_CommentId] ON [RequestAttachments] ([CommentId]);

CREATE INDEX [IX_RequestAttachments_ServiceRequestId] ON [RequestAttachments] ([ServiceRequestId]);

CREATE INDEX [IX_RequestComments_ServiceRequestId] ON [RequestComments] ([ServiceRequestId]);

CREATE INDEX [IX_RequestStatusHistory_ServiceRequestId] ON [RequestStatusHistory] ([ServiceRequestId]);

CREATE INDEX [IX_ServiceRequests_AssigneeId] ON [ServiceRequests] ([AssigneeId]);

CREATE INDEX [IX_ServiceRequests_CategoryId] ON [ServiceRequests] ([CategoryId]);

CREATE INDEX [IX_ServiceRequests_CreatedDate] ON [ServiceRequests] ([CreatedDate]);

CREATE INDEX [IX_ServiceRequests_Priority] ON [ServiceRequests] ([Priority]);

CREATE UNIQUE INDEX [IX_ServiceRequests_ReferenceNumber] ON [ServiceRequests] ([ReferenceNumber]);

CREATE INDEX [IX_ServiceRequests_RequesterId] ON [ServiceRequests] ([RequesterId]);

CREATE INDEX [IX_ServiceRequests_Status] ON [ServiceRequests] ([Status]);

CREATE INDEX [IX_SlaPolicies_Priority] ON [SlaPolicies] ([Priority]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261010043207_InitialCreate', N'10.0.12');

COMMIT;
GO

