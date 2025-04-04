using Business.Abstract;
using Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Business.Concrete;

// new() kısıtlaması kaldırıldı
public class GenericRepository<Tentity, Tcontext> : IGenericRepository<Tentity>
    where Tentity : class, new()
    where Tcontext : IdentityDbContext<AppUser, AppRole, int>
{
    // Context field'ı eklendi
    protected readonly Tcontext _context;

    // Constructor ile context enjeksiyonu
    public GenericRepository(Tcontext context)
    {
        _context = context;
    }

    // CreateContext metodu kaldırıldı

    public List<Tentity> GetAll(Expression<Func<Tentity, bool>>? filter = null)
    {
        // using bloğu kaldırıldı, _context kullanılıyor
        // Default olarak IsActive=true olan kayıtları getir
        var query = _context.Set<Tentity>().AsQueryable();

        // Eğer Tentity sınıfında IsActive property'si varsa, filtreleme yap
        // Dinamik sorgu yapılabilmesi için Expression kullanıldı
        var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
        if (isActiveProperty != null)
        {
            var parameter = Expression.Parameter(typeof(Tentity), "x"); // x => x.IsActive == true
            var property = Expression.Property(parameter, isActiveProperty); // x.IsActive
            var trueValue = Expression.Constant(true);
            var condition = Expression.Equal(property, trueValue);
            var lambda = Expression.Lambda<Func<Tentity, bool>>(condition, parameter);

            query = query.Where(lambda);
        }

        // diğer filtreler için 
        if (filter != null)
        {
            query = query.Where(filter);
        }

        return query.ToList();
    }

    public Tentity? Get(int id)
    {
        // using bloğu kaldırıldı, _context kullanılıyor
        var nesne = _context.Set<Tentity>().Find(id);

        // Eğer Tentity sınıfında IsActive property'si varsa ve false ise null dön
        if (nesne != null)
        {
            var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
            if (isActiveProperty != null)
            {
                // GetValue null dönebilir, cast etmeden önce kontrol et
                var isActiveValue = isActiveProperty.GetValue(nesne);
                if (isActiveValue is bool isActive && !isActive)
                {
                    return null;
                }
            }
        }

        return nesne;
    }

    public Tentity? Get(Expression<Func<Tentity, bool>>? filter)
    {
        // using bloğu kaldırıldı, _context kullanılıyor
        // Default olarak IsActive=true olan kayıtları getir
        var query = _context.Set<Tentity>().AsQueryable();

        // Eğer Tentity sınıfında IsActive property'si varsa, filtreleme yap
        var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
        if (isActiveProperty != null)
        {
            var parameter = Expression.Parameter(typeof(Tentity), "x");
            var property = Expression.Property(parameter, isActiveProperty);
            var trueValue = Expression.Constant(true);
            var condition = Expression.Equal(property, trueValue);
            var lambda = Expression.Lambda<Func<Tentity, bool>>(condition, parameter);

            query = query.Where(lambda);
        }

        // Belirtilen filtreyi de uygula
        // 'filter' null ise FirstOrDefault predicate olmadan çağrılır
        var nesne = filter == null ? query.FirstOrDefault() : query.FirstOrDefault(filter);
        return nesne;
    }

    public void Add(Tentity tentity)
    {
        // using bloğu kaldırıldı, _context kullanılıyor
        _context.Set<Tentity>().Add(tentity);
        _context.SaveChanges();
    }

    public void Update(Tentity tentity)
    {
        // using bloğu kaldırıldı, _context kullanılıyor
        _context.Set<Tentity>().Update(tentity);
        _context.SaveChanges();
    }

    public void Delete(Tentity tentity)
    {
        // using bloğu kaldırıldı, _context kullanılıyor
        var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
        if (isActiveProperty != null)
        {
            // Entity'nin state'ini değiştirmeden önce context'e attach etmek gerekebilir
            var entry = _context.Entry(tentity);
            if (entry.State == EntityState.Detached)
            {
                _context.Set<Tentity>().Attach(tentity);
            }
            isActiveProperty.SetValue(tentity, false);
            _context.Set<Tentity>().Update(tentity);
        }
        else
        {
            _context.Set<Tentity>().Remove(tentity);
        }
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        // using bloğu kaldırıldı, _context kullanılıyor
        var nesne = _context.Set<Tentity>().Find(id);
        if (nesne != null)
        {
            var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
            if (isActiveProperty != null)
            {
                isActiveProperty.SetValue(nesne, false);
                _context.Set<Tentity>().Update(nesne);
            }
            else
            {
                _context.Set<Tentity>().Remove(nesne);
            }
            _context.SaveChanges();
        }
    }
}