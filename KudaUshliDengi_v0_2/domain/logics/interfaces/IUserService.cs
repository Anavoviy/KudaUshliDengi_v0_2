using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.domain.logics.interfaces;

public interface IUserService
{
    Task<Result<User>> AddAsync(long tgUserId, string tgUsername, long tgChatId, CancellationToken ct = default);
    Task<Result<User>> ExistsAsync(long userTgId, CancellationToken ct = default);

    #region StatusStorage
    Task<Result> CheckStatusAsync(long userId, CancellationToken ct = default);
    #endregion
}