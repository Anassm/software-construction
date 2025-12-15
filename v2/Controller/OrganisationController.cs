using Microsoft.AspNetCore.Mvc;
using v2.Core.Interfaces;
using System.Security.Claims;
using System.Text;

namespace v2.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingController : ControllerBase
    {
        private readonly IOrganizations _organizationService;

        public ParkingController(IOrganizations organizationsService)
        {
            _organizationService = organizationsService;
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
                    404 => StatusCode(StatusCodes.Status404NotFound, result.data),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, result.data),
                    _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
                };
            }

            if (exportAsCsv)
            {

                dynamic data = result.data; 
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("Type,ID,StartDate,EndDate,TotalTime,ParkingLotID,Amount");

                foreach (var r in data.reservations)
                {
                    var totalTime = (r.EndDate - r.StartDate).TotalHours;
                    csvBuilder.AppendLine($"Reservation,{r.ID},{r.StartDate},{r.EndDate},{totalTime},{r.ParkingLotID},{r.TotalPrice}");
                }

                foreach (var s in data.sessions)
                {
                    var totalTime = (s.EndTime - s.StartTime).TotalHours;
                    csvBuilder.AppendLine($"Session,{s.ID},{s.StartTime},{s.EndTime},{totalTime},{s.ParkingLotID},{s.Price}");
                }

                var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                return File(bytes, "text/csv", "parking_actions.csv");
            }
            return Ok(result.data);
        }

    }
}

