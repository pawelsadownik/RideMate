using RideMate.Application.Dto;
using RideMate.Domain;

namespace RideMate.Application.Interfaces;

public interface ISafetyService
{
    Task BlockUserAsync(Guid blockerUserId, Guid blockedUserId, CancellationToken ct = default);
    Task UnblockUserAsync(Guid blockerUserId, Guid blockedUserId, CancellationToken ct = default);
    Task<List<Guid>> GetBlockedUserIdsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsBlockedAsync(Guid blockerUserId, Guid targetUserId, CancellationToken ct = default);
    Task<ReportDto> CreateReportAsync(Guid reporterUserId, ReportRequest request, CancellationToken ct = default);
    Task<List<ReportDto>> GetOpenReportsAsync(CancellationToken ct = default);
    Task SetReportStatusAsync(Guid reportId, ReportStatus status, CancellationToken ct = default);
}
