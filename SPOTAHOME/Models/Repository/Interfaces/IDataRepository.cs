namespace SPOTAHOME.Models.Repository.Interfaces
{
    public interface IDataRepository<TEntity>
    {
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> Get(long id);
        void Add(TEntity entity);
        void Update(TEntity entityToUpdate);
        void Delete(TEntity entity);
    }
}
