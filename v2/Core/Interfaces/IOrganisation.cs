using System;
using System.Threading.Tasks;
using v2.Core.DTOs;

namespace v2.Core.Interfaces
{
    public interface IOrganizations
    {
        Task<(int statusCode, object data)> CreateAsync(OrganizationCreateRequest dto);
        Task<(int statusCode, object data)> UpdateAsync(Guid id, OrganizationUpdateRequest dto);
        Task<(int statusCode, object data)> DeleteAsync(Guid id);
        Task<(int statusCode, object data)> GetAllAsync();
        Task<(int statusCode, object data)> GetByIdAsync(Guid id);
    }
}
