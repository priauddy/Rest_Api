namespace Court_Management.Services
{
    public interface IBaseService<T, TCreateDTO, TUpdateDTO>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> CreateAsync(TCreateDTO createDto);
        Task<T> UpdateAsync(int id, TUpdateDTO updateDto);
        Task<bool> DeleteAsync(int id);
    }
}
