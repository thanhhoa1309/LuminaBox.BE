using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Voucher;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IVoucherService
    {
        Task<VoucherResponse> CreatePlatformVoucherAsync(CreateVoucherRequest request);
        Task<VoucherResponse> CreateOrganizerVoucherAsync(CreateVoucherRequest request);
        Task<Pagination<VoucherResponse>> GetAllVouchersAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            VoucherScope? scope = null,
            VoucherStatus? status = null);
        Task<Pagination<VoucherResponse>> GetMyOrganizerVouchersAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            VoucherStatus? status = null);
        Task<VoucherResponse> GetVoucherByDetail(Guid id);
        Task<VoucherResponse> UpdateVoucherAsync(Guid id, UpdateVoucherRequest request);
        Task<VoucherResponse> UpdateVoucherStatusAsync(Guid id, UpdateVoucherStatusRequest request);
        Task DeleteVoucherAsync(Guid id);

        Task<List<VoucherResponse>> GetAvailableVouchersAsync(Guid? organizerId = null);
        Task<UserVoucherResponse> ClaimVoucherAsync(Guid voucherId);
        Task<List<UserVoucherResponse>> GetMyClaimedVouchersAsync();
        Task RemoveClaimedVoucherAsync(Guid userVoucherId);

        Task<Dictionary<int, OrderGroupVoucherResult>> ValidateAndCalculateOrderVouchersAsync(
            Guid userId,
            List<Guid>? requestedVoucherIds,
            IReadOnlyList<(int GroupIndex, Guid OrganizerId, decimal Subtotal)> groups,
            DateTime now);

        Task ConsumeVoucherAsync(
            UserVoucher userVoucher,
            Guid orderId,
            Guid userId,
            decimal discountAmount,
            DateTime now);

        Task RestoreOrderVouchersAsync(Order order, DateTime now);
    }
}

