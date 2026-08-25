using Gestion.Citas.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Gestion.Citas.Repositories.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity> CreateAsync(TEntity entity);
        Task UpdateAsync();
        Task<TEntity?> GetByIdAsync(int id);
        Task<(ICollection<TResult> Result, int TotalCount)> ListAsync<TResult>
        (
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> selector,
            int pageNumber = 1, int pageSize = 10
        );
        Task DeleteAsync(int id);
        Task<TEntity?> GetByPredicateAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
