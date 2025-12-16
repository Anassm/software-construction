using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using v2.Core.DTOs;
using v2.Core.Interfaces;

namespace v2.Controllers
{
    [ApiController]
    [Route("organizations")]
    [Authorize(Roles = "Admin")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizations _organizationService;

        public OrganizationController(IOrganizations organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrganizationCreateRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (statusCode, data) = await _organizationService.CreateAsync(dto);

            return statusCode switch
            {
                201 => Created($"/organizations/{((dynamic)data).organization.id}", data),
                409 => Conflict(data),
                _   => StatusCode(statusCode, data)
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var (statusCode, data) = await _organizationService.GetAllAsync();
            return StatusCode(statusCode, data);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var (statusCode, data) = await _organizationService.GetByIdAsync(id);

            return statusCode switch
            {
                200 => Ok(data),
                404 => NotFound(data),
                _   => StatusCode(statusCode, data)
            };
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationUpdateRequest dto)
        {
            var (statusCode, data) = await _organizationService.UpdateAsync(id, dto);

            return statusCode switch
            {
                200 => Ok(data),
                404 => NotFound(data),
                409 => Conflict(data),
                _   => StatusCode(statusCode, data)
            };
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (statusCode, data) = await _organizationService.DeleteAsync(id);

            return statusCode switch
            {
                200 => Ok(data),
                404 => NotFound(data),
                409 => Conflict(data),
                _   => StatusCode(statusCode, data)
            };
        }
    }
}
