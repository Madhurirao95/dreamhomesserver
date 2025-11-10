namespace DREAMHOMES.Models.Repository.Interfaces
{
    public interface IDataRepository<TEntity, TId>
    {
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> Get(TId id);
        void Add(TEntity entity);
        void Update(TEntity entityToUpdate);
        void Delete(TEntity entity);
    }
}
