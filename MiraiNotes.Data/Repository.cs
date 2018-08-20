using Microsoft.EntityFrameworkCore;
using MiraiNotes.DataService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MiraiNotes.Data
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected readonly DbContext _context;
        private DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            bool result = false;
            try
            {
                result = await _dbSet
                        .AsNoTracking()
                        .Where(predicate)
                        .CountAsync() == 1;
            }
            catch (Exception)
            {
                //TODO: ADD SOME LOGGING..
            }
            return result;
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return await _dbSet
                    .Where(predicate)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return Enumerable.Empty<TEntity>();
            }
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(predicate);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<TEntity> GetAsync(
            Expression<Func<TEntity, bool>> filter,
            string includeProperties = "")
        {
            return (await GetAsync(filter, null, includeProperties))
                .FirstOrDefault();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            try
            {
                IQueryable<TEntity> query = _dbSet;
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
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
            catch (Exception)
            {
                return Enumerable.Empty<TEntity>();
            }
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsNoTrackingAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            try
            {
                IQueryable<TEntity> query = _dbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }

                if (orderBy != null)
                {
                    return await orderBy(query)
                        .AsNoTracking()
                        .ToListAsync();
                }
                else
                {
                    return await query
                        .AsNoTracking()
                        .ToListAsync();
                }
            }
            catch (Exception)
            {
                return Enumerable.Empty<TEntity>();
            }
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                        .ToListAsync();
            }
            catch (Exception)
            {
                return Enumerable.Empty<TEntity>();
            }
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsNoTrackingAsync()
        {
            try
            {
                return await _dbSet
                        .AsNoTracking()
                        .ToListAsync();
            }
            catch (Exception)
            {
                return Enumerable.Empty<TEntity>();
            }
        }

        public virtual async Task RemoveAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                Remove(entity);
        }

        public virtual async Task RemoveAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            var entities = await GetAsync(filter, null, string.Empty);
            if (entities.Count() > 0)
                RemoveRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
