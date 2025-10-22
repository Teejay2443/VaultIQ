using VaultIQ.Dtos.Requests;
using VaultIQ.Dtos.Responses;

namespace VaultIQ.Interfaces.Services
{
    public interface IDataRequestService
    {
        Task<ResponseModel<Guid>> CreateDataRequestAsync(Guid businessId, CreateDataRequestDto dto);
        Task<ResponseModel> UpdateRequestStatusAsync(UpdateDataRequestStatusDto dto);
        Task<ResponseModel<IEnumerable<DataRequestResponseDto>>> GetUserRequestsAsync(string userEmail);
        Task<ResponseModel<IEnumerable<DataRequestResponseDto>>> GetBusinessRequestsAsync(Guid businessId);
    }
}
