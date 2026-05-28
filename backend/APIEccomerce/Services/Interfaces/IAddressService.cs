using APIEccomerce.Models.DTOs;

namespace APIEccomerce.Services.Interfaces
{
    public interface IAddressService
    {
        Task<List<AddressDto>> GetByUserId(int userId);
        Task<AddressDto?> GetDefaultAddress(int userId);
        Task<AddressDto> Create(
            int userId,
            CreateAddressDto dto);

        Task<AddressDto?> Update(
            int userId,
            int addressId,
            CreateAddressDto dto);

        Task<bool> Delete(
            int userId,
            int addressId);
    }
}