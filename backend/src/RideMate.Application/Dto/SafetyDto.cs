namespace RideMate.Application.Dto;

public sealed record BlockRequest(Guid BlockedUserId);
public sealed record ReportRequest(Guid ReportedUserId, string Reason, string? Description, Guid? MessageId);
public sealed record ReportDto(
    Guid Id,
    Guid ReporterUserId,
    Guid ReportedUserId,
    string Reason,
    string? Description,
    string Status,
    DateTime CreatedAt
);
