using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.StallRental;
using LuminaBox.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminaBox.Application.Interface
{
    public interface IStallRentalService
    {
        Task<Pagination<StallRentalResponse>> GetAllAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RentalStatus? status = null,
            Guid? stallId = null,
            Guid? organizerId = null);

        Task<Pagination<StallRentalResponse>> GetMineAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RentalStatus? status = null);

        Task<Pagination<StallRentalResponse>> GetByStallAsync(
            Guid stallId,
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RentalStatus? status = null);

        Task<StallRentalResponse> GetByIdAsync(Guid id);
        Task<StallRentalResponse> TerminateAsync(Guid id);

        Task<int> SyncStatusesAsync();
    }
}
