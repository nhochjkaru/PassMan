using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.Application.Interface
{
    public interface IAsyncRepository<T> where T : EntityBase
    {
        IQueryable<T> GetAll();
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null,
                                        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                        string includeString = null,
                                        bool disableTracking = true);
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null,
                                       Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                       List<Expression<Func<T, object>>> includes = null,
                                       bool disableTracking = true);
        Task<T> GetByIdAsync(long id);
        Task<IEnumerable<T>> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "");
        IQueryable<T1> Get<TResult, T1>(
           Expression<Func<T, bool>> filter = null,
           Expression<Func<T, TResult>> orderBy = null,
           Func<IQueryable<T>, IQueryable<T1>> selector = null,
           string includeProperties = "");
        T GetFirstOrDefault(Expression<Func<T, bool>> predicate = null,
                            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                            bool disableTracking = true,
                            bool ignoreQueryFilters = false);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null,
                                       Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                       Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                                       bool disableTracking = true,
                                       bool ignoreQueryFilters = false);
        Task<T> ForUpdate(Expression<Func<T, bool>> filter = null);
        Task<T> AddAsync(T entity);
        Task<T> AddNoSaveAsync(T entity);
        Task AddRange(params T[] entities);
        Task AddRange(IEnumerable<T> entities);
        Task AddRangeAsync(params T[] entities);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default(CancellationToken));
        Task AddRangeNoSaveAsync(params T[] entities);
        Task AddRangeNoSaveAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateAsync(T entity);
        Task UpdateRange(params T[] entities);
        Task UpdateRange(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteRange(params T[] entities);
        Task DeleteRange(IEnumerable<T> entities);
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAync();
        IQueryable<T> FromSql(string sql);
        Task ExecuteSQL(string sql, Array parameter);
        void getIdentityBySequence(string CESQ_VIR_CIF, ref string p_cif_no);
        public DbConnection GetConnection();
    }
}
