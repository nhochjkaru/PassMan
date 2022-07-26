using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using PasswordManager.Application.Interface;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.Infrastructure.Repositories
{
    public class RepositoryBase<T> : IAsyncRepository<T> where T : EntityBase
    {
        protected readonly AppDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        public RepositoryBase(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }
        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }
        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            //Expression<Func<T, bool>> other = s => s.branch_code == _dbContext.CurrentBranch;
            //var dt = AndExpression(predicate, other);
            return await _dbContext.Set<T>().Where(predicate).ToListAsync();
        }
        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null,
                                                     Func<IQueryable<T>,
                                                     IOrderedQueryable<T>> orderBy = null,
                                                     string includeString = null,
                                                     bool disableTracking = true)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (disableTracking) query = query.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);

            //Expression<Func<T, bool>> other = s => s.branch_code == _dbContext.CurrentBranch;
            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync();
            return await query.ToListAsync();
        }
        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null,
                                                     Func<IQueryable<T>,
                                                     IOrderedQueryable<T>> orderBy = null,
                                                     List<Expression<Func<T, object>>> includes = null,
                                                     bool disableTracking = true)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (disableTracking) query = query.AsNoTracking();

            // query = query.Where(p => p.branch_code == _dbContext.CurrentBranch);

            if (includes != null) query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync();
            return await query.ToListAsync();
        }
        public virtual async Task<T> GetByIdAsync(long id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        public async Task<IEnumerable<T>> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }
        public IQueryable<T1> Get<TResult, T1>(
           Expression<Func<T, bool>> filter = null,
           Expression<Func<T, TResult>> orderBy = null,
           Func<IQueryable<T>, IQueryable<T1>> selector = null,
           string includeProperties = "")
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                query = query.OrderByDescending(orderBy);
            }
            if (selector != null)
            {
                return selector(query);
            }
            else
            {
                return (IQueryable<T1>)query.ToList();
            }
        }
        public virtual T GetFirstOrDefault(Expression<Func<T, bool>> predicate = null,
                                           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                           Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                                           bool disableTracking = true,
                                           bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = _dbSet;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }
            if (include != null)
            {
                query = include(query);
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }
            if (orderBy != null)
            {
                return orderBy(query).FirstOrDefault();
            }
            else
            {
                return query.FirstOrDefault();
            }
        }
        public virtual async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null,
                                                            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                                            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
                                                            bool disableTracking = true,
                                                            bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (orderBy != null)
            {
                return await orderBy(query).FirstOrDefaultAsync();
            }
            else
            {
                return await query.FirstOrDefaultAsync();
            }
        }
        public virtual async Task<T> ForUpdate(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ForUpdate().FirstOrDefaultAsync();
        }
        public async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<T> AddNoSaveAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }
        public async Task AddRange(params T[] entities)
        {
            _dbSet.AddRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AddRangeAsync(params T[] entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AddRangeNoSaveAsync(params T[] entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }
        public async Task AddRangeNoSaveAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }
        public async Task UpdateAsync(T entity)
        {
            var checkLocal = _dbContext.Set<T>()
                .Local
                .FirstOrDefault(p => p.Id == entity.Id);
            if (checkLocal != null)
            {
                _dbContext.Entry(checkLocal).State = EntityState.Detached;
            }
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateNoSaveAsync(T entity)
        {
            var checkLocal = _dbContext.Set<T>()
                .Local
                .FirstOrDefault(p => p.Id == entity.Id);
            if (checkLocal != null)
            {
                _dbContext.Entry(checkLocal).State = EntityState.Detached;
            }
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
        public async Task UpdateRange(params T[] entities)
        {
            _dbSet.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteRange(params T[] entities)
        {
            _dbSet.RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        #region Bo sung
        public Expression<Func<T, bool>> AndExpression(Expression<Func<T, bool>> left,
                                                       Expression<Func<T, bool>> right)
        {
            var invoked = Expression.Invoke(right, left.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
            (Expression.AndAlso(left.Body, invoked), left.Parameters);
        }
        public IDbContextTransaction BeginTransaction()
        {
            return _dbContext.Database.BeginTransaction();
        }
        public async Task<IDbContextTransaction> BeginTransactionAync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }
        public virtual IQueryable<T> FromSql(string sql)
        {
            return _dbSet.FromSqlRaw(sql);
        }

        public async Task ExecuteSQL(string sql, Array parameter)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql, parameter);
        }
        public void getIdentityBySequence(string CESQ_VIR_CIF, ref string p_cif_no)
        {
            string l_sequence_name = "";
            Int64 sequennum = 0;
            l_sequence_name = CESQ_VIR_CIF;

            //TODO can them check da tao sequence hay chua o day?

            //TODO tao so giao dich
            //TODO bat exception sau
            var result = new NpgsqlParameter("", System.Data.SqlDbType.Decimal);
            result.Direction = System.Data.ParameterDirection.Output;
            _dbContext.Database.ExecuteSqlRaw("SELECT nextval('" + l_sequence_name + "')", result);
            sequennum = (Int64)result.Value;

            p_cif_no = sequennum.ToString().PadLeft(6, '0');
        }
        public DbConnection GetConnection() => _dbContext.Database.GetDbConnection();
        #endregion
    }
}
