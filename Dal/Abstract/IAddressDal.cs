using Business.Abstract;
using Data.Entities;

namespace Dal.Abstract;

public interface IAddressDal : IGenericRepository<Address>
{
    List<Address> GetAddressesByUserId(int userId);
    Address? GetDefaultAddress(int userId);
    void SetDefaultAddress(int addressId, int userId);
}