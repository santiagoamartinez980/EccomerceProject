using APIEccomerce.Models;

namespace APIEccomerce.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<List<Address>> GetByUserId(int userId);

        Task<Address?> GetById(int addressId);

        Task<Address> Create(Address address);

        Task<Address?> Update(Address address);

        Task<bool> Delete(int addressId);

        Task<Address?> GetDefaultAddress(int userId);

        Task ClearDefaultAddresses(int userId);
    }
}