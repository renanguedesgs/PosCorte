using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Services
{
    /// <summary>
    /// Repositório in-memory para desenvolvimento sem banco de dados.
    /// Substituir por implementação real com EF Core na fase de persistência.
    /// </summary>
    public class RepositorioMemoria<T> : IRepositorio<T> where T : class
    {
        private readonly List<T> _data = new();
        private int _nextId = 1;

        private static int GetId(T entity)
        {
            var prop = typeof(T).GetProperty("Id");
            return prop != null ? (int)(prop.GetValue(entity) ?? 0) : 0;
        }

        private static void SetId(T entity, int id)
        {
            var prop = typeof(T).GetProperty("Id");
            prop?.SetValue(entity, id);
        }

        public Task<T?> GetByIdAsync(int id)
        {
            var entity = _data.FirstOrDefault(e => GetId(e) == id);
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_data.AsEnumerable());
        }

        public Task<T> AddAsync(T entity)
        {
            SetId(entity, _nextId++);
            _data.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<T> UpdateAsync(T entity)
        {
            var id = GetId(entity);
            var index = _data.FindIndex(e => GetId(e) == id);
            if (index >= 0)
                _data[index] = entity;
            return Task.FromResult(entity);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var entity = _data.FirstOrDefault(e => GetId(e) == id);
            if (entity == null) return Task.FromResult(false);
            _data.Remove(entity);
            return Task.FromResult(true);
        }

        public Task<bool> SaveChangesAsync()
        {
            return Task.FromResult(true);
        }
    }
}
