using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class Report : BaseEntity
{
    public Guid ReporterUserId { get; private set; }
    public Guid ReportedUserId { get; private set; }
    public Guid? MessageId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ReportStatus Status { get; private set; } = ReportStatus.Open;

    public User? Reporter { get; private set; }
    public User? Reported { get; private set; }
    public Message? Message { get; private set; }

    private Report() { }

    public static Report Create(Guid reporterUserId, Guid reportedUserId, string reason, string? description, Guid? messageId)
    {
        return new Report
        {
            ReporterUserId = reporterUserId,
            ReportedUserId = reportedUserId,
            Reason = reason,
            Description = description,
            MessageId = messageId,
            Status = ReportStatus.Open
        };
    }

    public void SetStatus(ReportStatus status) { Status = status; Touch(); }
}
