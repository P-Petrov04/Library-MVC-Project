using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Common.Repositories
{
    public class BaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _items;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _items = _context.Set<T>();
        }



        public List<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = _items;

            if (filter != null)
                query = query.Where(filter);

            return query.ToList();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> filter)
        {
            return _items.FirstOrDefault(filter);
        }

        public void Add(T item)
        {
            _items.Add(item);
            _context.SaveChanges();
        }

        public void Update(T item)
        {
            _items.Update(item);
            _context.SaveChanges();
        }

        public void Delete(T item)
        {
            _items.Remove(item);
            _context.SaveChanges();
        }
    }
}
