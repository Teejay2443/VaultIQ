using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaultIQ.Dtos.Requests;
using VaultIQ.Interfaces.Services;

namespace VaultIQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataRequestController : ControllerBase
    {
        private readonly IDataRequestService _dataRequestService;
        private readonly ILogger<DataRequestController> _logger;

        public DataRequestController(IDataRequestService dataRequestService, ILogger<DataRequestController> logger)
        {
            _dataRequestService = dataRequestService;
            _logger = logger;
        }

        // -----------------------------------------------------------
        // BUSINESS: Create Data Request
        // -----------------------------------------------------------
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateDataRequest([FromBody] CreateDataRequestDto dto)
        {
            try
            {
                var businessIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(businessIdClaim))
                    return Unauthorized(new { message = "Invalid or missing business token." });

                var businessId = Guid.Parse(businessIdClaim);
                var result = await _dataRequestService.CreateDataRequestAsync(businessId, dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating data request.");
                return StatusCode(500, new { message = "An error occurred while creating the data request." });
            }
        }

        // -----------------------------------------------------------
        // USER: Approve or Decline Request
        // -----------------------------------------------------------
        [HttpPut("update-status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateDataRequestStatusDto dto)
        {
            try
            {
                var result = await _dataRequestService.UpdateRequestStatusAsync(dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating data request status.");
                return StatusCode(500, new { message = "An error occurred while updating the request." });
            }
        }

        // -----------------------------------------------------------
        // USER: Get Requests for This User
        // -----------------------------------------------------------
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserRequests()
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(new { message = "Invalid or missing token." });

                var result = await _dataRequestService.GetUserRequestsAsync(userEmail);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user requests.");
                return StatusCode(500, new { message = "An error occurred while retrieving user requests." });
            }
        }

        // -----------------------------------------------------------
        // BUSINESS: Get Requests by This Business
        // -----------------------------------------------------------
        [HttpGet("business")]
        [Authorize]
        public async Task<IActionResult> GetBusinessRequests()
        {
            try
            {
                var businessIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(businessIdClaim))
                    return Unauthorized(new { message = "Invalid business token." });

                var businessId = Guid.Parse(businessIdClaim);
                var result = await _dataRequestService.GetBusinessRequestsAsync(businessId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching business requests.");
                return StatusCode(500, new { message = "An error occurred while retrieving business requests." });
            }
        }
    }
}
