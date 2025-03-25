using System.Linq.Expressions;
using Business.Abstract;
using Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Business.Concrete;

public class GenericRepository<Tentity, Tcontext> : IGenericRepository<Tentity> where Tentity : class, new()
    where Tcontext : IdentityDbContext<AppUser, AppRole, int>, new()
{
    public List<Tentity> GetAll(Expression<Func<Tentity, bool>> filter = null)
    {
        using (var db = new Tcontext())
        {
            // Default olarak IsActive=true olan kayıtları getir
            var query = db.Set<Tentity>().AsQueryable();

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
    }

    public Tentity Get(int id)
    {
        using (var db = new Tcontext())
        {
            var nesne = db.Set<Tentity>().Find(id);
            
            // Eğer Tentity sınıfında IsActive property'si varsa ve false ise null dön
            if (nesne != null)
            {
                var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
                if (isActiveProperty != null)
                {
                    bool isActive = (bool)isActiveProperty.GetValue(nesne);
                    if (!isActive)
                    {
                        return null;
                    }
                }
            }
            
            return nesne;
        }
    }

    public Tentity Get(Expression<Func<Tentity, bool>> filter)
    {
        using (var db = new Tcontext())
        {
            // Default olarak IsActive=true olan kayıtları getir
            var query = db.Set<Tentity>().AsQueryable();
            
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
            var nesne = query.FirstOrDefault(filter);
            return nesne;
        }
    }

    public void Add(Tentity tentity)
    {
        using (var db = new Tcontext())
        {
            db.Set<Tentity>().Add(tentity);
            db.SaveChanges();
        }
    }

    public void Update(Tentity tentity)
    {
        using (var db = new Tcontext())
        {
            db.Set<Tentity>().Update(tentity);
            db.SaveChanges();
        }
    }

    public void Delete(Tentity tentity)
    {
    
        var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
        if (isActiveProperty != null)
        {
            using (var db = new Tcontext())
            {
                isActiveProperty.SetValue(tentity, false);
                db.Set<Tentity>().Update(tentity);
                db.SaveChanges();
            }
        }
        else
        {
       
            using (var db = new Tcontext())
            {
                db.Entry(tentity).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }
    }

    public void Delete(int id)
    {
        using (var db = new Tcontext())
        {
            var nesne = db.Set<Tentity>().Find(id);
            if (nesne != null)
            {
             
                var isActiveProperty = typeof(Tentity).GetProperty("IsActive");
                if (isActiveProperty != null)
                {
                    isActiveProperty.SetValue(nesne, false);
                    db.Set<Tentity>().Update(nesne);
                }
                else
                {
                  
                    db.Set<Tentity>().Remove(nesne);
                }
                db.SaveChanges();
            }
        }
    }
}