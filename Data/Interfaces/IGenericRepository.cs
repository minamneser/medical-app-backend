namespace ModelTest.Data.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> FindByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);

    }
}
