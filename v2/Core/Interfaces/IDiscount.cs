using System;
using System.Threading.Tasks;
using v2.Core.DTOs;

namespace v2.Core.Interfaces
{
    public interface IDiscounts
    {
        Task<(int statusCode, object data)> CreateAsync(DiscountCreateRequest dto, string adminIdentityUserId);
        Task<(int statusCode, object data)> UpdateAsync(Guid id, DiscountUpdateRequest dto, string adminIdentityUserId);
        Task<(int statusCode, object data)> DeactivateAsync(Guid id, string adminIdentityUserId);
        Task<(int statusCode, object data)> UpdateExpiryAsync(Guid id, DateTime? expiryDate, string adminIdentityUserId);
        Task<(int statusCode, object data)> LinkUsersAsync(Guid id, DiscountLinkUsersRequest dto, string adminIdentityUserId);
        Task<(int statusCode, object data)> ValidateAndApplyAsync(DiscountApplyRequest dto, string identityUserId);
        Task<( DiscountStatistieksResponse data, int statusCode,  object message)> GetStatisticsAsync(string? filter = null, string? orderBy = null);
        Task<(int statusCode, object data)> GetAllActiveCodesAsync(string adminIdentityUserId);
        Task<(int statusCode, object data)> GetUsedCodesAsync(Guid? discountCodeId, string adminIdentityUserId);
    }
}
