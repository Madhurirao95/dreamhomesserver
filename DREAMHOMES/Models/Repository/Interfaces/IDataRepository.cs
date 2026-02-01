namespace DREAMHOMES.Models.Repository.Interfaces
{
    public interface IDataRepository<TEntity, TId>
    {
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> Get(TId id);
        Task Add(TEntity entity);
        Task Update(TEntity entityToUpdate);
        Task Delete(TEntity entity);
    }
}
