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
            return filter == null ? db.Set<Tentity>().ToList() : db.Set<Tentity>().Where(filter).ToList();
        }
    }

    public Tentity Get(int id)
    {
        using (var db = new Tcontext())
        {
            var nesne = db.Set<Tentity>().Find(id);
            return nesne;
        }
    }

    public Tentity Get(Expression<Func<Tentity, bool>> filter)
    {
        using (var db = new Tcontext())
        {
            var nesne = db.Set<Tentity>().FirstOrDefault(filter);
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
        using (var db = new Tcontext())
        {
            db.Entry(tentity).State = EntityState.Deleted;
            db.SaveChanges();
            // db.Set<Tentity>().Remove(tentity); // bu da olur ama sanırım doğru bir yöntem değil
            // db.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        using (var db = new Tcontext())
        {
            var nesne = db.Set<Tentity>().Find(id);
            db.Set<Tentity>().Remove(nesne);
            db.SaveChanges();
        }
    }
}