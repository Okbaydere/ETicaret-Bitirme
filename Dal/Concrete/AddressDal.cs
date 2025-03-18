using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;

namespace Dal.Concrete;

public class AddressDal : GenericRepository<Address, ETicaretContext>, IAddressDal
{
    public List<Address> GetAddressesByUserId(int userId)
    {
        using var context = new ETicaretContext();
        return context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Title)
            .ToList();
    }

    public Address GetDefaultAddress(int userId)
    {
        using var context = new ETicaretContext();
        return context.Addresses
            .FirstOrDefault(a => a.UserId == userId && a.IsDefault);
    }

    public void SetDefaultAddress(int addressId, int userId)
    {
        using var context = new ETicaretContext();

        // İşlem başlat
        using var transaction = context.Database.BeginTransaction();

        try
        {
            // Önce tüm adresleri bul
            var userAddresses = context.Addresses.Where(a => a.UserId == userId).ToList();

            // Hepsinin default durumunu false yap
            foreach (var address in userAddresses)
            {
                address.IsDefault = false;
            }

            // Seçilen adresi default yap
            var defaultAddress = userAddresses.FirstOrDefault(a => a.Id == addressId);
            if (defaultAddress != null)
            {
                defaultAddress.IsDefault = true;
            }

            context.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}