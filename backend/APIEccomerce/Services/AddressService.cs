using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services.Interfaces;

namespace APIEccomerce.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo;
        private readonly IGoogleGeocodingService _geocoding;

        public AddressService(
            IAddressRepository repo,
            IGoogleGeocodingService geocoding)
        {
            _repo = repo;
            _geocoding = geocoding;
        }

        public async Task<List<AddressDto>> GetByUserId(int userId)
        {
            var addresses = await _repo.GetByUserId(userId);
            return addresses.Select(Map).ToList();
        }

        public async Task<AddressDto?> GetDefaultAddress(int userId)
        {
            var address = await _repo.GetDefaultAddress(userId);
            return address is null ? null : Map(address);
        }

        public async Task<AddressDto> Create(int userId, CreateAddressDto dto)
        {
            if (dto.IsDefault)
                await _repo.ClearDefaultAddresses(userId);

            var existing = await _repo.GetByUserId(userId);
            var isDefault = dto.IsDefault || existing.Count == 0;

            // Si el frontend no envió coordenadas, las pide a Google
            if (dto.Latitude == 0 && dto.Longitude == 0)
            {
                var geo = await _geocoding.GeocodeAddressAsync(
                    dto.AddressLine, dto.City, dto.Department);

                if (geo is not null)
                {
                    dto.Latitude = geo.Latitude;
                    dto.Longitude = geo.Longitude;
                    dto.FormattedAddress = geo.FormattedAddress;
                    dto.PostalCode = geo.PostalCode;
                    dto.PlaceId = geo.PlaceId;
                }
            }

            var address = new Address
            {
                UserId = userId,
                AddressLine = dto.AddressLine,
                City = dto.City,
                Department = dto.Department,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Phone = dto.Phone,
                Notes = dto.Notes,
                PlaceId = dto.PlaceId,
                FormattedAddress = dto.FormattedAddress,
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.Create(address);
            return Map(created);
        }

        public async Task<AddressDto?> Update(
            int userId, int addressId, CreateAddressDto dto)
        {
            var existing = await _repo.GetById(addressId);

            // Verificar que la dirección pertenece al usuario
            if (existing is null || existing.UserId != userId)
                return null;

            if (dto.IsDefault)
                await _repo.ClearDefaultAddresses(userId);

            existing.AddressLine = dto.AddressLine;
            existing.City = dto.City;
            existing.Department = dto.Department;
            existing.Country = dto.Country;
            existing.PostalCode = dto.PostalCode;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;
            existing.Phone = dto.Phone;
            existing.Notes = dto.Notes;
            existing.PlaceId = dto.PlaceId;
            existing.FormattedAddress = dto.FormattedAddress;
            existing.IsDefault = dto.IsDefault;

            var updated = await _repo.Update(existing);
            return updated is null ? null : Map(updated);
        }

        public async Task<bool> Delete(int userId, int addressId)
        {
            var address = await _repo.GetById(addressId);

            // Verificar que la dirección pertenece al usuario
            if (address is null || address.UserId != userId)
                return false;

            return await _repo.Delete(addressId);
        }

        private static AddressDto Map(Address a) => new()
        {
            AddressId = a.AddressId,
            AddressLine = a.AddressLine,
            City = a.City,
            Department = a.Department,
            Country = a.Country,
            PostalCode = a.PostalCode,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            Phone = a.Phone,
            Notes = a.Notes,
            PlaceId = a.PlaceId,
            FormattedAddress = a.FormattedAddress,
            IsDefault = a.IsDefault
        };
    }
}