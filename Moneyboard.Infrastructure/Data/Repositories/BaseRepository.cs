﻿using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moneyboard.Core.Entities.BankCardEntity;
using Moneyboard.Core.Entities.UserProjectEntity;
using Moneyboard.Core.Interfaces;
using Moneyboard.Core.Interfaces.Repository;
using System.Linq.Expressions;

namespace Moneyboard.Infrastructure.Data.Repositories
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class, IBaseEntity
    {
        protected readonly MoneyboardDb _dbContext;
        protected readonly DbSet<TEntity> _dbSet;
        public BaseRepository(MoneyboardDb dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }
        public async Task<TEntity> GetFirstBySpecAsync(ISpecification<TEntity> specification)
        {
            var res = await ApplySpecification(specification).FirstOrDefaultAsync();
            return res;

        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            var evaluator = new SpecificationEvaluator();
            return evaluator.GetQuery(_dbSet, specification);
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetByKeyAsync<TKey>(TKey key)
        {
            return await _dbSet.FindAsync(key);
        }
        public async Task<TEntity> GetByPairOfKeysAsync<TFirstKey, TSecondKey>
            (TFirstKey firstKey, TSecondKey secondKey)
        {
            return await _dbSet.FindAsync(firstKey, secondKey);
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            return (await _dbSet.AddAsync(entity)).Entity;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            await Task.Run(() => _dbContext.Entry(entity).State = EntityState.Modified);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await Task.Run(() => _dbSet.Remove(entity));
        }
        /*public async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            await Task.Run(() => _dbSet.RemoveRange(entities));
        }*/

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<TEntity> entities)
        {
            await _dbContext.AddRangeAsync(entities);
        }
        public async Task<IEnumerable<TEntity>> GetListAsync(
    Expression<Func<TEntity,
    bool>> filter = null,
    Func<IQueryable<TEntity>,
    IOrderedQueryable<TEntity>> orderBy = null,
    string includeProperties = null)
        {
            // Створення запиту до бази даних на основі параметрів
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Виконання запиту та отримання результатів
            return await query.ToListAsync();
        }

        // перенести в окремий репозиторій або узагальнити
        public async Task<BankCard> GetBankCardByProjectIdAsync(int projectId)
        {
            // Знаходимо проект з включеною інформацією про банківську картку
            var project = _dbContext.Project
                .Include(p => p.BankCard)
                .FirstOrDefault(p => p.ProjectId == projectId);

            if (project != null)
            {
                return project.BankCard;
            }

            return null;
        }
        public async Task<UserProject> GetUserProjectAsync(string userId, int projectId)
        {

            // Виконайте запит до бази даних
            var userProject = await _dbContext.UserProject
                .Where(up => up.UserId == userId && up.ProjectId == projectId)
                .FirstOrDefaultAsync();

            return userProject;
        }
        public async Task<BankCard> GetByCardNumberAsync(string cardNumber)
        {
            if (typeof(TEntity) == typeof(BankCard))
            {
                var bankCardEntity = await _dbSet.SingleOrDefaultAsync(x => ((BankCard)(object)x).CardNumber == cardNumber);
                return bankCardEntity as BankCard;
            }
            else
            {
                throw new ArgumentException("GetByCardNumberAsync can only be used with BankCard entities.");
            }
        }



    }
}
