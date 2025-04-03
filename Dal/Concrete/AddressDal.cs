using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Concrete;

public class AddressDal : GenericRepository<Address, ETicaretContext>, IAddressDal
{
    public AddressDal(ETicaretContext context) : base(context)
    {
    }

    public List<Address> GetAddressesByUserId(int userId)
    {
        return _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Title)
            .ToList();
    }

    public Address GetDefaultAddress(int userId)
    {
        return _context.Addresses
            .FirstOrDefault(a => a.UserId == userId && a.IsDefault);
    }

    public void SetDefaultAddress(int addressId, int userId)
    {
        using var transaction = _context.Database.BeginTransaction();
        
        try
        {
            _context.Database.ExecuteSqlRaw(
                "UPDATE Addresses SET IsDefault = 0 WHERE UserId = {0}", userId);
            
            _context.Database.ExecuteSqlRaw(
                "UPDATE Addresses SET IsDefault = 1 WHERE Id = {0} AND UserId = {1}", 
                addressId, userId);
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}