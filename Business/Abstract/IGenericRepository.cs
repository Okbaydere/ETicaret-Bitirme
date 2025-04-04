using System.Linq.Expressions;

namespace Business.Abstract;

public interface IGenericRepository<Tentity> where Tentity : class, new()
{
    List<Tentity> GetAll(Expression<Func<Tentity, bool>>? filter = null);
    Tentity? Get(int id);
    Tentity? Get(Expression<Func<Tentity, bool>>? filter);
    void Add(Tentity tentity);
    void Update(Tentity tentity);
    void Delete(Tentity tentity);
    void Delete(int id);
}