using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class BaseRepository<T>
        where T : class
    {
        private DbContext Context { get; set; }
        private DbSet<T> Items { get; set; }

        public BaseRepository()
        {
            Context = new ApplicationDbContext();
            Items = Context.Set<T>();
        }

        public List<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = Items;

            if (filter != null)
                query = query.Where(filter);

            return Items.ToList();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> filter)
        {
            return Items.FirstOrDefault(filter);
        }

        public void Add(T item)
        {
            Items.Add(item);
            Context.SaveChanges();
        }

        public void Update(T item)
        {
            Items.Update(item);
            Context.SaveChanges();
        }

        public void Delete(T item)
        {
            Items.Remove(item);
            Context.SaveChanges();
        }
    }
}
