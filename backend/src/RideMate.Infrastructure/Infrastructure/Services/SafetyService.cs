using Microsoft.EntityFrameworkCore;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Domain;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class SafetyService : ISafetyService
{
    private readonly RideMateDbContext _db;

    public SafetyService(RideMateDbContext db) => _db = db;

    public async Task BlockUserAsync(Guid blockerUserId, Guid blockedUserId, CancellationToken ct = default)
    {
        if (blockerUserId == blockedUserId)
            throw new InvalidOperationException("Nie można zablokować samego siebie.");

        var existing = await _db.Blocks
            .FirstOrDefaultAsync(b => b.BlockerUserId == blockerUserId && b.BlockedUserId == blockedUserId, ct);

        if (existing is not null) return;

        var block = Block.Create(blockerUserId, blockedUserId);
        _db.Blocks.Add(block);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UnblockUserAsync(Guid blockerUserId, Guid blockedUserId, CancellationToken ct = default)
    {
        var block = await _db.Blocks
            .FirstOrDefaultAsync(b => b.BlockerUserId == blockerUserId && b.BlockedUserId == blockedUserId, ct)
            ?? throw new InvalidOperationException("Blokada nie została znaleziona.");

        _db.Blocks.Remove(block);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Guid>> GetBlockedUserIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Blocks
            .Where(b => b.BlockerUserId == userId)
            .Select(b => b.BlockedUserId)
            .ToListAsync(ct);
    }

    public async Task<bool> IsBlockedAsync(Guid blockerUserId, Guid targetUserId, CancellationToken ct = default)
    {
        return await _db.Blocks
            .AnyAsync(b => b.BlockerUserId == blockerUserId && b.BlockedUserId == targetUserId, ct);
    }

    public async Task<ReportDto> CreateReportAsync(Guid reporterUserId, ReportRequest request, CancellationToken ct = default)
    {
        var report = Report.Create(reporterUserId, request.ReportedUserId, request.Reason, request.Description, request.MessageId);
        _db.Reports.Add(report);
        await _db.SaveChangesAsync(ct);

        return Map(report);
    }

    private static ReportDto Map(Report r) => new(
        r.Id,
        r.ReporterUserId,
        r.ReportedUserId,
        r.Reason,
        r.Description,
        r.Status.ToString(),
        r.CreatedAt
    );

    public async Task<List<ReportDto>> GetOpenReportsAsync(CancellationToken ct = default)
    {
        var reports = await _db.Reports
            .Include(r => r.Reporter).ThenInclude(u => u.Profile)
            .Include(r => r.Reported).ThenInclude(u => u.Profile)
            .Where(r => r.Status == ReportStatus.Open)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reports.Select(Map).ToList();
    }

    public async Task SetReportStatusAsync(Guid reportId, ReportStatus status, CancellationToken ct = default)
    {
        var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
            ?? throw new InvalidOperationException("Zgłoszenie nie zostało znalezione.");

        report.SetStatus(status);
        await _db.SaveChangesAsync(ct);
    }
}
