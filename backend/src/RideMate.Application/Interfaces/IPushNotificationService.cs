namespace RideMate.Application.Interfaces;

public interface IPushNotificationService
{
    Task SendMatchNotificationAsync(Guid toUserId, string fromNickname, CancellationToken ct = default);
    Task SendMessageNotificationAsync(Guid toUserId, string fromNickname, string preview, CancellationToken ct = default);
}
