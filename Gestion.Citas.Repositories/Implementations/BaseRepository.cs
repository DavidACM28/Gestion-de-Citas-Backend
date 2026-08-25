using Gestion.Citas.DataAccess;
using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Gestion.Citas.Repositories.Implementations
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly AppointmentsDbContext _context;
        public BaseRepository(AppointmentsDbContext context)
        {
            _context = context;
        }

        //Método de Creación
        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            var result = await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        //Metodo de actualizacion
        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }

        //Metodo de obtencion por id
        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(p => p.Active && p.Id == id);
        }

        //Metodo de obtencion por predicado
        public async Task<TEntity?> GetByPredicateAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }

        //Metodo de listado con paginación
        public async Task<(ICollection<TResult> Result, int TotalCount)> ListAsync<TResult>
        (
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> selector,
            int pageNumber = 1, int pageSize = 10
        )
        {
            var result = await _context.Set<TEntity>()
                .Where(predicate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();
            var total = await _context.Set<TEntity>()
                .Where(predicate)
                .CountAsync();
            return (result, total);
        }

        //Metodo de soft delete
        public async Task DeleteAsync(int id)
        {
            await _context.Set<TEntity>()
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(p =>
                p.SetProperty(e => e.Active, false));
        }
    }
}
