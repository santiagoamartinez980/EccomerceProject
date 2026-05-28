using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Address>> GetByUserId(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();
        }

        public async Task<Address?> GetById(int addressId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == addressId);
        }

        public async Task<Address> Create(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return address;
        }

        public async Task<Address?> Update(Address address)
        {
            var existing = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == address.AddressId);

            if (existing == null)
                return null;

            existing.AddressLine = address.AddressLine;
            existing.City = address.City;
            existing.Department = address.Department;
            existing.Country = address.Country;
            existing.PostalCode = address.PostalCode;
            existing.Latitude = address.Latitude;
            existing.Longitude = address.Longitude;
            existing.Phone = address.Phone;
            existing.Notes = address.Notes;
            existing.PlaceId = address.PlaceId;
            existing.FormattedAddress = address.FormattedAddress;
            existing.IsDefault = address.IsDefault;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> Delete(int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == addressId);

            if (address == null)
                return false;

            _context.Addresses.Remove(address);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Address?> GetDefaultAddress(int userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.IsDefault);
        }

        public async Task ClearDefaultAddresses(int userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            foreach (var address in addresses)
            {
                address.IsDefault = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}