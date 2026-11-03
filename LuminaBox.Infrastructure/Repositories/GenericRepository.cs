using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Interface;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Interfaces;
using LuminaBox.Infrastructure.Persistence;

namespace LuminaBox.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IGenericRepository{TEntity}"/>.
    /// Never saves changes; commit through <see cref="IUnitOfWork.SaveChangesAsync"/>.
    /// </summary>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly LuminaBoxDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        private readonly ICurrentTime _currentTime;
        private readonly IClaimsService _claimsService;

        public GenericRepository(LuminaBoxDbContext context, ICurrentTime currentTime, IClaimsService claimsService)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
            _currentTime = currentTime;
            _claimsService = claimsService;
        }

        // Audit FKs (e.g. Voucher.CreatedBy) must be null rather than Guid.Empty for anonymous calls
        private Guid? CurrentUserId
        {
            get
            {
                var userId = _claimsService.GetCurrentUserId;
                return userId == Guid.Empty ? null : userId;
            }
        }

        #region Create

        /// <summary>Tracks a new entity and fills its create audit fields.</summary>
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var now = _currentTime.GetCurrentTime();
            SetCreateAudit(entity, now, CurrentUserId);

            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <summary>Tracks new entities and fills their create audit fields.</summary>
        public async Task AddRangeAsync(List<TEntity> entities)
        {
            var now = _currentTime.GetCurrentTime();
            var userId = CurrentUserId;

            foreach (var entity in entities)
            {
                SetCreateAudit(entity, now, userId);
            }

            await _dbSet.AddRangeAsync(entities);
        }

        #endregion

        #region Read

        /// <summary>Gets an entity by id, or null if it does not exist or is soft-deleted.</summary>
        public Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
        {
            return ApplyIncludes(_dbSet, includes)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>Gets all non-deleted entities matching the optional predicate.</summary>
        public Task<List<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return ApplyPredicate(ApplyIncludes(_dbSet, includes), predicate)
                .ToListAsync();
        }

        /// <summary>Gets the first non-deleted entity matching the optional predicate, or null.</summary>
        public Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return ApplyPredicate(ApplyIncludes(_dbSet, includes), predicate)
                .FirstOrDefaultAsync();
        }

        /// <summary>Returns a composable query over non-deleted entities.</summary>
        public IQueryable<TEntity> GetQueryable() => _dbSet;

        #endregion

        #region Read including soft-deleted

        /// <summary>Gets entities including soft-deleted ones (bypasses the global query filter).</summary>
        public Task<List<TEntity>> GetAllIncludingDeletedAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return ApplyPredicate(ApplyIncludes(_dbSet.IgnoreQueryFilters(), includes), predicate)
                .ToListAsync();
        }

        /// <summary>Checks existence including soft-deleted rows (bypasses the global query filter).</summary>
        public Task<bool> AnyIncludingDeletedAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return ApplyPredicate(_dbSet.IgnoreQueryFilters(), predicate).AnyAsync();
        }

        #endregion

        #region Update

        /// <summary>Marks an entity as modified and fills its update audit fields.</summary>
        public Task<bool> Update(TEntity entity)
        {
            var now = _currentTime.GetCurrentTime();
            SetUpdateAudit(entity, now, CurrentUserId);

            _dbSet.Update(entity);
            return Task.FromResult(true);
        }

        /// <summary>Marks entities as modified and fills their update audit fields.</summary>
        public Task<bool> UpdateRange(List<TEntity> entities)
        {
            var now = _currentTime.GetCurrentTime();
            var userId = CurrentUserId;

            foreach (var entity in entities)
            {
                SetUpdateAudit(entity, now, userId);
            }

            _dbSet.UpdateRange(entities);
            return Task.FromResult(true);
        }

        #endregion

        #region Soft delete

        /// <summary>Flags an entity as deleted and fills its delete audit fields.</summary>
        public Task<bool> SoftRemove(TEntity entity)
        {
            var now = _currentTime.GetCurrentTime();
            SetDeleteAudit(entity, now, CurrentUserId);

            _dbSet.Update(entity);
            return Task.FromResult(true);
        }

        /// <summary>Flags entities as deleted and fills their delete audit fields.</summary>
        public Task<bool> SoftRemoveRange(List<TEntity> entities)
        {
            var now = _currentTime.GetCurrentTime();
            var userId = CurrentUserId;

            foreach (var entity in entities)
            {
                SetDeleteAudit(entity, now, userId);
            }

            _dbSet.UpdateRange(entities);
            return Task.FromResult(true);
        }

        /// <summary>Loads non-deleted entities by id and flags them as deleted. Returns false if none matched.</summary>
        public async Task<bool> SoftRemoveRangeById(List<Guid> entitiesId)
        {
            if (entitiesId.Count == 0)
            {
                return false;
            }

            var entities = await _dbSet
                .Where(e => entitiesId.Contains(e.Id))
                .ToListAsync();

            var now = _currentTime.GetCurrentTime();
            var userId = CurrentUserId;

            foreach (var entity in entities)
            {
                SetDeleteAudit(entity, now, userId);
            }

            return entities.Count > 0;
        }

        #endregion

        #region Hard delete

        /// <summary>
        /// Physically deletes entities matching the predicate, soft-deleted rows included.
        /// Returns false if none matched.
        /// </summary>
        public async Task<bool> HardRemove(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await _dbSet
                .IgnoreQueryFilters()
                .Where(predicate)
                .ToListAsync();

            _dbSet.RemoveRange(entities);
            return entities.Count > 0;
        }

        /// <summary>Physically deletes the given entities.</summary>
        public Task<bool> HardRemoveRange(List<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.FromResult(true);
        }

        #endregion

        #region Helpers

        private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, Expression<Func<TEntity, object>>[] includes)
        {
            return includes.Aggregate(query, (current, include) => current.Include(include));
        }

        private static IQueryable<TEntity> ApplyPredicate(IQueryable<TEntity> query, Expression<Func<TEntity, bool>>? predicate)
        {
            return predicate is null ? query : query.Where(predicate);
        }

        private static void SetCreateAudit(TEntity entity, DateTime now, Guid? userId)
        {
            entity.CreatedAt = now;
            entity.UpdatedAt = now;

            if (entity.CreatedBy is null || entity.CreatedBy == Guid.Empty)
            {
                entity.CreatedBy = userId;
            }
        }

        private static void SetUpdateAudit(TEntity entity, DateTime now, Guid? userId)
        {
            entity.UpdatedAt = now;
            entity.UpdatedBy = userId;
        }

        private static void SetDeleteAudit(TEntity entity, DateTime now, Guid? userId)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = now;
            entity.DeletedBy = userId;
        }

        #endregion
    }
}
