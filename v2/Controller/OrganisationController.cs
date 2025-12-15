using Microsoft.AspNetCore.Mvc;
using v2.Core.Interfaces;
using System.Security.Claims;
using System.Text;
using v2.Core.DTOs;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace v2.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizations _organizationService;

        public OrganizationController(IOrganizations organizationsService)
        {
            _organizationService = organizationsService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] OrganizationCreateRequest dto)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _organizationService.CreateAsync(dto);

            return result.statusCode switch
            {
                201 => StatusCode(StatusCodes.Status201Created, result.data),
                409 => StatusCode(StatusCodes.Status409Conflict, result.data),
                500 => StatusCode(StatusCodes.Status500InternalServerError, result.data),
                _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
            };
        }

        // GET: api/parking/actions
        [HttpGet("actions")]
        public async Task<IActionResult> GetParkingActions(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? parkingLotId = null,
            [FromQuery] float? minAmount = null,
            [FromQuery] float? maxAmount = null,
            [FromQuery] bool exportAsCsv = false)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _organizationService.GetParkingActions(
                identityUserId, startDate, endDate, parkingLotId, minAmount, maxAmount, exportAsCsv);

            if (result.statusCode != 200)
            {
                return result.statusCode switch
                {
                    400 => StatusCode(StatusCodes.Status400BadRequest, result.data),
                    403 => StatusCode(StatusCodes.Status403Forbidden, result.data),
                    404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, result.data),
                    _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
                };
            }

            if (exportAsCsv)
            {

                var data = result.data as OrganizationActionsResponse;
                if (data == null)
                {
                    return StatusCode(500, new { error = "Invalid response format." });
                }
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("Type,ID,StartDate,EndDate,TotalTime,ParkingLotID,Amount");

                foreach (var r in data.Reservations)
                {
                    var totalTime = (r.EndDate - r.StartDate)?.TotalHours ?? 0;
                    csvBuilder.AppendLine(
                        $"Reservation,{r.ID},{r.StartDate},{r.EndDate},{totalTime},{r.ParkingLotID},{r.TotalPrice}"
                    );
                }

                foreach (var s in data.Sessions)
                {
                    var totalTime = (s.EndTime - s.StartTime)?.TotalHours ?? 0;
                    csvBuilder.AppendLine(
                        $"Session,{s.ID},{s.StartTime},{s.EndTime},{totalTime},{s.ParkingLotID},{s.Price}"
                    );
                }


                var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
                    .Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString()))
                    .ToArray();

                // Use Append instead of Add
                Response.Headers.Append("Content-Disposition", "attachment; filename=parking_actions.csv");
                Response.Headers.Append("Content-Type", "application/csv");

                return File(bytes, "text/csv");
            }
            return Ok(result.data);
        }
        
        [HttpPut("AssignToOrganization")]
        public async Task<IActionResult> AssignUserToOrganization([FromBody] AssignUserDto dto)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (identityUserId == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

            var result = await _organizationService.assignUserToOrganization(identityUserId, dto.OrganizationId);

            return result.statusCode switch
            {
                200 => StatusCode(StatusCodes.Status200OK, result.data),
                400 => StatusCode(StatusCodes.Status400BadRequest, result.data),
                404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                500 => StatusCode(StatusCodes.Status500InternalServerError, result.data),
                _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
            };
        }
    }
}

