using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ACRM.mobile.DataAccess.Local
{
    public class LocalRepository<T, TContext> : ILocalRepository<T> where T : class where TContext : DbContext
    {
        protected DbContext _context;
        public LocalRepository(TContext context)
        {
            _context = context;
        }

        public virtual T Add(T entity)
        {
            return _context
                .Add(entity)
                .Entity;
        }

        public virtual void AddRange(List<T> entities)
        {
            _context.AddRange(entities);
        } 

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>()
                .AsQueryable()
                .Where(predicate).ToList();
        }

        public virtual T Get(Guid id)
        {
            return _context.Find<T>(id);
        }

        public virtual T Get(int id)
        {
            return _context.Find<T>(id);
        }

        public virtual T Get(string stringId)
        {
            try
            {
                return _context.Find<T>(stringId);
            }
            catch (Exception ex)
            {
                return null; 
            }
        }

        public virtual IEnumerable<T> All()
        {
            return _context.Set<T>()
                .ToList();
        }

        public virtual T Update(T entity)
        {
            _context.Update(entity);
            return entity;
        }

        public virtual T Remove(T entity)
        {
            return _context.Remove(entity).Entity;
        }

        public virtual void RemoveAll()
        {
            _context.RemoveRange(All());
        }
    }
}
