using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ACRM.mobile.DataAccess
{
    public interface ILocalRepository<T>
    {
        T Add(T entity);
        void AddRange(List<T> entities);
        void RemoveAll();
        T Update(T entity);
        T Remove(T entity);
        T Get(Guid id);
        T Get(int id);
        T Get(string stringId);
        IEnumerable<T> All();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        // Do not add save method. save should be coordinated from a unitofwork.
    }
}
