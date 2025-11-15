using VaultIQ.Dtos.Requests;
using VaultIQ.Dtos.Responses;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Interfaces.Services;
using VaultIQ.Models;

namespace VaultIQ.Services
{
    public class DataRequestService : IDataRequestService
    {
        private readonly IDataRequestRepository _dataRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDocumentRespository _documentRepository; 
        private readonly IEmailServices _emailServices;
        private readonly ILogger<DataRequestService> _logger;

        public DataRequestService(
            IDataRequestRepository dataRequestRepository,
            IUserRepository userRepository,
            IDocumentRespository documentRespository, 
            IEmailServices emailServices,
            ILogger<DataRequestService> logger)
        {
            _dataRequestRepository = dataRequestRepository;
            _userRepository = userRepository;
            _documentRepository = documentRespository; 
            _emailServices = emailServices;
            _logger = logger;
        }

        // -----------------------------------------------------------
        // CREATE DATA REQUEST
        // -----------------------------------------------------------
        public async Task<ResponseModel<Guid>> CreateDataRequestAsync(Guid businessId, CreateDataRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Business {BusinessId} requesting data for {Email}", businessId, dto.UserEmail);

                var user = await _userRepository.GetByEmailAsync(dto.UserEmail);
                if (user == null)
                    return ResponseModel<Guid>.Failure("No user found with this email.");

                if (string.IsNullOrWhiteSpace(dto.FileName))
                    return ResponseModel<Guid>.Failure("File name must be provided.");

                // ✅ Check if file exists under that user
                var file = await _documentRepository.GetFileByUserAndNameAsync(user.Id, dto.FileName.Trim());
                if (file == null)
                    return ResponseModel<Guid>.Failure("Requested file not found under this user's data.");

                var request = new DataRequest
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    UserEmail = dto.UserEmail,
                    FileName = dto.FileName.Trim(),
                    FileUrl =  "Default",
                    PurposeOfAccess = dto.PurposeOfAccess.Trim(),
                    AccessDurationInHours = dto.AccessDurationInHours > 0 ? dto.AccessDurationInHours : 24,
                    Status = "Pending",
                    RequestedAt = DateTime.UtcNow
                }; 

                await _dataRequestRepository.AddDataRequestAsync(request);
                await _dataRequestRepository.SaveChangesAsync();

                // ✅ Email Notification to user
                string subject = "VaultIQ Data Access Request";
                string body = $@"
                    <h3>VaultIQ Data Access Request</h3>
                    <p>Dear {user.FullName},</p>
                    <p>A business has requested access to your data file <b>{request.FileName}</b>.</p>
                    <p><b>Purpose:</b> {request.PurposeOfAccess}</p>
                    <p><b>Duration:</b> {request.AccessDurationInHours} hours</p>
                    <p>Please login to your VaultIQ dashboard to approve or decline this request.</p>
                    <br/>
                    <p>VaultIQ Team</p>";

                await _emailServices.SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation("Data request created and email sent to user {Email}", user.Email);

                return ResponseModel<Guid>.Success(request.Id, "Data request created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating data request.");
                return ResponseModel<Guid>.Failure("An error occurred while creating the data request.");
            }
        }

        // -----------------------------------------------------------
        // UPDATE REQUEST STATUS (Approve / Decline)
        // -----------------------------------------------------------
        public async Task<ResponseModel> UpdateRequestStatusAsync(UpdateDataRequestStatusDto dto)
        {
            try
            {
                var request = await _dataRequestRepository.GetByIdAsync(dto.RequestId);
                if (request == null)
                    return ResponseModel.Failure("Data request not found.");

                if (request.Status != "Pending")
                    return ResponseModel.Failure("This request has already been processed.");

                if (dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                {
                    // ✅ Get file to include its URL for approved access
                    var user = await _userRepository.GetByEmailAsync(request.UserEmail);
                    var file = await _documentRepository.GetFileByUserAndNameAsync(user.Id, request.FileName);
                    if (file == null)
                        return ResponseModel.Failure("Cannot approve request. File not found.");

                    request.Status = "Approved";
                    request.ExpiresAt = DateTime.UtcNow.AddHours(request.AccessDurationInHours);
                    request.FileUrl = file.FileUrl;
                }
                else if (dto.Status.Equals("Declined", StringComparison.OrdinalIgnoreCase))
                {
                    request.Status = "Declined";
                }
                else
                {
                    return ResponseModel.Failure("Invalid status. Use 'Approved' or 'Declined'.");
                }

                await _dataRequestRepository.UpdateDataRequestAsync(request);
                await _dataRequestRepository.SaveChangesAsync();

                _logger.LogInformation("Request {RequestId} updated to {Status}", request.Id, request.Status);
                return ResponseModel.Success($"Request {request.Status.ToLower()} successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating data request status.");
                return ResponseModel.Failure("An error occurred while updating the request status.");
            }
        }

        // -----------------------------------------------------------
        // GET USER REQUESTS
        // -----------------------------------------------------------
        public async Task<ResponseModel<IEnumerable<DataRequestResponseDto>>> GetUserRequestsAsync(string userEmail)
        {
            var requests = await _dataRequestRepository.GetByUserEmailAsync(userEmail);
            if (!requests.Any())
                return ResponseModel<IEnumerable<DataRequestResponseDto>>.Failure("No requests found.");

            var response = requests.Select(r => new DataRequestResponseDto
            {
                Id = r.Id,
                BusinessName = r.Business?.CompanyName ?? "Unknown",
                UserEmail = r.UserEmail,
                FileName = r.FileName,
                PurposeOfAccess = r.PurposeOfAccess,
                AccessDurationInHours = r.AccessDurationInHours,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                ExpiresAt = r.ExpiresAt
            });

            return ResponseModel<IEnumerable<DataRequestResponseDto>>.Success(response, "Requests retrieved successfully.");
        }

        // -----------------------------------------------------------
        // GET BUSINESS REQUESTS
        // -----------------------------------------------------------
        public async Task<ResponseModel<IEnumerable<DataRequestResponseDto>>> GetBusinessRequestsAsync(Guid businessId)
        {
            var requests = await _dataRequestRepository.GetByBusinessIdAsync(businessId);
            if (!requests.Any())
                return ResponseModel<IEnumerable<DataRequestResponseDto>>.Failure("No requests found.");

            var response = requests.Select(r => new DataRequestResponseDto
            {
                Id = r.Id,
                BusinessName = r.Business?.CompanyName ?? "Unknown",
                UserEmail = r.UserEmail,
                FileName = r.FileName,
                PurposeOfAccess = r.PurposeOfAccess,
                AccessDurationInHours = r.AccessDurationInHours,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                ExpiresAt = r.ExpiresAt,
                FileUrl = r.Status == "Approved" ? r.FileUrl : null // ✅ only show file link if approved
            });

            return ResponseModel<IEnumerable<DataRequestResponseDto>>.Success(response, "Requests retrieved successfully.");
        }
    }
}






